using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using prakt_8_trpo.Models;
using Microsoft.AspNetCore.Authorization;

namespace prakt_8_trpo.Controllers
{
    [ApiController]
    [Route("api/v1/tasks")]
    [Authorize]
    public class TasksController :Controller
    {
        private static TasksDbContext _context = new TasksDbContext( );

        [HttpGet]
        public IActionResult GetTasks()
        {
            var currentUser = User.Identity?.Name;
            return Ok(_context.Tasks.Where(t => t.OwnerLogin == currentUser).ToList());
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            return Ok(_context.Tasks.ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null) return NotFound();

            if (task.OwnerLogin != User.Identity?.Name && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(task);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Models.Task task)
        {
            if (task == null) 
                return BadRequest();
            
            task.Id = 0;
            task.OwnerLogin = User.Identity?.Name;
            task.IsCompleted = false;

            _context.Tasks.Add(task);
            _context.SaveChanges();

            return Created($"/api/v1/tasks/{task.Id}", task);
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Models.Task taskUpdate)
        {
            var task = _context.Tasks.Find(id);
            if (task == null) 
                return NotFound();

            if (task.OwnerLogin != User.Identity?.Name && !User.IsInRole("Admin"))
                return Forbid();

            task.Title = taskUpdate.Title;
            task.Description = taskUpdate.Description;
            task.IsCompleted = taskUpdate.IsCompleted;

            _context.Tasks.Update(task);
            _context.SaveChanges();

            return Ok(task);
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var task = _context.Tasks.Find(id);

            if (task == null)
                return NotFound();

            if (task.OwnerLogin != User.Identity?.Name && !User.IsInRole("Admin"))
                return Forbid();

            _context.Tasks.Remove(task);
            _context.SaveChanges();

            return NoContent();
        }

    }
}
