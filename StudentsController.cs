using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Classes7.Models;
using Classes7.Models.DTOs;
using Classes7.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Classes7.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private IStudentDbService _service;
        private IConfiguration Configuration; 

        public StudentsController(IStudentDbService service, IConfiguration configuration)
        {
            _service = service;
            Configuration = configuration;
        }
        
        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok("Get Students");
        }

        [HttpPost]
        public IActionResult Login(LoginRequestDto request)
        {

            Student student = _service.GetStudent(request.IndexNumber);

            if (student == null)
            {
                return NotFound("Index number not found !");
            }

            if (!Validate(request.Password, student.Salt, student.Password))
            {
                return Unauthorized("Incorrect password !");
            }

            string accessToken = GenerateAccessToken(student);
            string refreshToken = GenerateAndSaveRefreshToken(student.IndexNumber);
            
            return Ok(new
            {
                accessToken,    
                refreshToken
            });
        }
        
        public bool Validate(string value, string salt, string hashedPassword)
            => CreateHash(value, salt) == hashedPassword;

        public string CreateHash(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                password: value,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return Convert.ToBase64String(valueBytes);
        }
        
        public string GenerateAccessToken(Student student)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, student.IndexNumber),
                new Claim(ClaimTypes.Name, student.FirstName),
                new Claim(ClaimTypes.Surname, student.LastName),
                new Claim(ClaimTypes.Role, "employee")    
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["MySecret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateAndSaveRefreshToken(string indexNumber)
        {
            var refreshToken = Guid.NewGuid().ToString();
            
            _service.SaveRefreshToken(indexNumber, refreshToken);

            return refreshToken;
        }
        
        [HttpGet("refresh-token/{refreshToken}")]
        public IActionResult RefreshToken(string refreshToken)
        {
            Student student = _service.GetStudentByRefreshToken(refreshToken);

            if (student == null)
            {
                return NotFound("Refresh token not found !");
            }
            
            string accessToken = GenerateAccessToken(student);
            string newRefreshToken = GenerateAndSaveRefreshToken(student.IndexNumber);
            
            return Ok(new
            {
                accessToken,    
                newRefreshToken
            });
        }
       
        [HttpGet("salt")]
        public IActionResult CreateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Ok(Convert.ToBase64String(randomBytes));
            }
        }
    }
}