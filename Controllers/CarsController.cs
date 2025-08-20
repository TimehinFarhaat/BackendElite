using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly ICarService _carService;

    public CarsController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _carService.GetAllCarsAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null) throw new KeyNotFoundException("Car not found.");
            return Ok(car);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("createCar")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Creates a new car with an image upload")]
    [AdminOnly]
    public async Task<IActionResult> CreateCar([FromForm] CreateCarRequest request)
    {
        try
        {
            var car = await _carService.CreateCarWithImageAsync(request);
            return Ok(car);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{carId:guid}/updateCar")]
    [Consumes("multipart/form-data")]

    public async Task<IActionResult> Update(Guid carId, [FromForm] UpdateCarRequest dto)
    {
        try
        {
            var updatedCar = await _carService.UpdateCarWithImageAsync(carId, dto, isAdmin: true);
            return Ok(updatedCar);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{carId:guid}/deleteCar")]
    [AdminOnly]
    public async Task<IActionResult> DeleteCar(Guid carId)
    {
        try
        {
            await _carService.DeleteCarAsync(carId, isAdmin: true);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{carId:guid}/carImage/{imageId:guid}")]

    public async Task<IActionResult> DeleteCarImage(Guid carId, Guid imageId)
    {
        //var isAdmin = HttpContext.Session.GetString("IsAdmin");
        //if (isAdmin != "true")
        //    return Unauthorized("You must be logged in as admin.");


        try
        {
            var updatedCar = await _carService.DeleteCarImageAsync(carId, imageId, isAdmin: true);
            return Ok(updatedCar);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
     
    private IActionResult HandleError(Exception ex)
    {
        return ex switch
        {
            ArgumentException => BadRequest(new { message = ex.Message }),
            InvalidOperationException => BadRequest(new { message = ex.Message }),
            UnauthorizedAccessException => Unauthorized(new { message = ex.Message }),
            KeyNotFoundException => NotFound(new { message = ex.Message }),
            _ => StatusCode(500, new { message = ex.Message })
        };
    }
}