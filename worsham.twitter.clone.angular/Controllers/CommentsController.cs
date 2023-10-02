using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;
using IAuthorizationService = worsham.twitter.clone.angular.Services.IAuthorizationService;

namespace worsham.twitter.clone.angular.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : TwitterController
    {
        private readonly TwitterCloneContext _context;

        public CommentsController(TwitterCloneContext context, ILogger<CommentsController> logger, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
        }

        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comments>>> GetComments()
        {
            if (_context.Comments == null)
            {
                return NotFound();
            }
            return await _context.Comments.ToListAsync();
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comments>> GetComments(int id)
        {
            if (_context.Comments == null)
            {
                return NotFound();
            }
            var comments = await _context.Comments.FindAsync(id);

            if (comments == null)
            {
                return NotFound();
            }

            return comments;
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComments(int id, Comments comments)
        {
            if (id != comments.Id)
            {
                return BadRequest();
            }

            _context.Entry(comments).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Comments>> PostComments(Comments comments)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'TwitterCloneContext.Comments'  is null.");
            }
            _context.Comments.Add(comments);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComments", new { id = comments.Id }, comments);
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComments(int id)
        {
            if (_context.Comments == null)
            {
                return NotFound();
            }
            var comments = await _context.Comments.FindAsync(id);
            if (comments == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comments);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Creates a new comment and adds it to the database.
        /// </summary>
        /// <param name="comments">The comment to be created.</param>
        /// <returns>
        /// If the ModelState is valid, redirects to the "Index" action of the "Tweets" controller.
        /// If the ModelState is invalid, logs validation errors, and redirects to the "Index"
        /// action of the "Tweets" controller.
        /// </returns>
        /// <remarks>
        /// If the ModelState is invalid, this method logs validation errors using the provided
        /// logger.
        /// </remarks>
        [HttpPost("create")]
        // public async Task<IActionResult> Create([Bind("Id,Content,OriginalTweetId,CommenterId")] Comments comments)
        public async Task<IActionResult> Create([FromBody] Comments comments)
        {
            // Retrieve the JWT token from the Authorization header
            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader == null)
            {
                throw new ArgumentNullException(nameof(authorizationHeader));
            }
            var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

            if (user.Id < 1)
            {
                _logger.LogInformation("User is not logged in.");
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "User is not logged in." });
            }

            if (ModelState.IsValid)
            {
                _context.Add(comments);
                await _context.SaveChangesAsync();
                return Json(new TwitterApiActionResult { Success = true });
            }
            else
            {
                // If ModelState is invalid, log validation errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("Model Error: {ErrorMessage}", error.ErrorMessage);
                    }
                }
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "Comment creation failed due to invalid user input." });
            }
        }

        private bool CommentsExists(int id)
        {
            return (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
