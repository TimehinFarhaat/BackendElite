using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

public class ClarifaiService
{
    private readonly ClarifaiSettings _settings;
    private readonly HttpClient _httpClient;

    public ClarifaiService(IOptions<ClarifaiSettings> settings, HttpClient httpClient)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Key {_settings.ApiKey}");
    }

    public async Task<bool> ValidateCarImageAsync(Stream imgStream)
    {
        imgStream.Position = 0;
        if (!await IsImageSharpEnoughAsync(imgStream))
            throw new ArgumentException("Image appears blurry or low resolution.");

        imgStream.Position = 0;
        if (!await IsCarImageAsync(imgStream))
            throw new ArgumentException("Clarifai did not detect a car in the image.");

        return true;
    }

    private async Task<bool> IsImageSharpEnoughAsync(Stream imgStream, int minWidth = 300, int minHeight = 300, double varianceThreshold = 120.0)
    {
        imgStream.Position = 0;

        using var image = await Image.LoadAsync<Rgba32>(imgStream);

        if (image.Width < minWidth || image.Height < minHeight)
            return false;

        const int maxDim = 800;
        if (Math.Max(image.Width, image.Height) > maxDim)
        {
            var scale = (double)maxDim / Math.Max(image.Width, image.Height);
            var newW = (int)(image.Width * scale);
            var newH = (int)(image.Height * scale);
            image.Mutate(x => x.Resize(newW, newH));
        }

        using var gray = image.CloneAs<L8>();

        double sum = 0, sumSq = 0;
        long count = 0;

        gray.ProcessPixelRows(accessor =>
        {
            for (int y = 1; y < accessor.Height - 1; y++)
            {
                var prevRow = accessor.GetRowSpan(y - 1);
                var curRow = accessor.GetRowSpan(y);
                var nextRow = accessor.GetRowSpan(y + 1);

                for (int x = 1; x < accessor.Width - 1; x++)
                {
                    int center = curRow[x].PackedValue;
                    int left = curRow[x - 1].PackedValue;
                    int right = curRow[x + 1].PackedValue;
                    int top = prevRow[x].PackedValue;
                    int bottom = nextRow[x].PackedValue;

                    int lap = (4 * center) - left - right - top - bottom;
                    double resp = Math.Abs(lap);

                    sum += resp;
                    sumSq += resp * resp;
                    count++;
                }
            }
        });

        if (count == 0) return false;

        double mean = sum / count;
        double variance = (sumSq / count) - (mean * mean);

        return variance >= varianceThreshold;
    }

    private async Task<bool> IsCarImageAsync(Stream imageStream)
    {
        var clarifaiUrl = $"https://api.clarifai.com/v2/users/{_settings.UserId}/apps/{_settings.AppId}/models/{_settings.ModelId}/outputs";

        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms);
        var base64Image = Convert.ToBase64String(ms.ToArray());

        var payload = new
        {
            inputs = new[]
            {
                new
                {
                    data = new
                    {
                        image = new { base64 = base64Image }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(clarifaiUrl, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);

        var concepts = doc.RootElement
            .GetProperty("outputs")[0]
            .GetProperty("data")
            .GetProperty("concepts");

        foreach (var concept in concepts.EnumerateArray())
        {
            var name = concept.GetProperty("name").GetString();
            var value = concept.GetProperty("value").GetDouble();

            if (value >= _settings.ConfidenceThreshold &&
                (name.Contains("car", StringComparison.OrdinalIgnoreCase) ||
                 name.Contains("vehicle", StringComparison.OrdinalIgnoreCase) ||
                 name.Contains("automobile", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
