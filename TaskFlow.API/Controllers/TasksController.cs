using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Требует аутентификации по умолчанию
    public class TasksController : ControllerBase
    {
        // Эндпоинт для пользователей с ролью "User"
        [HttpGet("user-tasks")]
        [Authorize(Policy = "UserOnly")]
        public IActionResult GetUserTasks()
        {
            return Ok("Список задач для обычных пользователей");
        }

        // Эндпоинт для администраторов
        [HttpGet("admin/stats")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetAdminStats()
        {
            return Ok("Статистика для администраторов");
        }
    }
}
