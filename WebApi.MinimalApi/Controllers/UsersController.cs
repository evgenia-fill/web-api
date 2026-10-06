using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet("{userId:guid}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user == null)
            return NotFound();
        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
    }

    [HttpPost]
    [Produces("application/json", "application/xml")]
    public IActionResult CreateUser([FromBody] CreateUserDto dto)
    {
        if (dto.Login != null && !dto.Login.All(char.IsLetterOrDigit))
        {
            ModelState.AddModelError(
                "Login",
                "Login must consist of letters or digits");
        }

        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        var user = _mapper.Map<UserEntity>(dto);

        var createdUser = _userRepository.Insert(user);

        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = createdUser.Id },
            createdUser.Id);
    }

    [HttpPut("{userId}")]
    public IActionResult UpdateUser([FromRoute] string userId, [FromBody] UpdateUserDto dto)
    {
        if (!Guid.TryParse(userId, out var id) || dto == null)
            return BadRequest();
        
        var user = _userRepository.FindById(id);

        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        if (user == null)
        {
            user = _mapper.Map(dto, new UserEntity(id));
        }
        else
        {
            _mapper.Map(dto, user);
        }

        _userRepository.UpdateOrInsert(user, out var isInserted);
        if (isInserted)
            return CreatedAtRoute(
                nameof(GetUserById),
                new { userId },
                id
            );
        return NoContent();
    }
}