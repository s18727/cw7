using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Classes7.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    [Authorize(Roles = "employee")]
    public class EnrollmentsController : ControllerBase
    {
        
        
        [HttpGet]
        public IActionResult EnrollStudent()
        {
            return Ok("Successfully student enrollment !");
        }

        [HttpGet]
        [Route("promotions")]
        public IActionResult PromoteStudents()
        {
            return Ok("Successfully promoting students !");
        }
    }
}