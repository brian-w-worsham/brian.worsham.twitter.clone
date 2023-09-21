using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowsController : ControllerBase
    {
        private readonly TwitterCloneContext _context;

        public FollowsController(TwitterCloneContext context)
        {
            _context = context;
        }

        // GET: api/Follows
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Follows>>> GetFollows()
        {
          if (_context.Follows == null)
          {
              return NotFound();
          }
            return await _context.Follows.ToListAsync();
        }

        // GET: api/Follows/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Follows>> GetFollows(int id)
        {
          if (_context.Follows == null)
          {
              return NotFound();
          }
            var follows = await _context.Follows.FindAsync(id);

            if (follows == null)
            {
                return NotFound();
            }

            return follows;
        }

        // PUT: api/Follows/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFollows(int id, Follows follows)
        {
            if (id != follows.Id)
            {
                return BadRequest();
            }

            _context.Entry(follows).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FollowsExists(id))
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

        // POST: api/Follows
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Follows>> PostFollows(Follows follows)
        {
          if (_context.Follows == null)
          {
              return Problem("Entity set 'TwitterCloneContext.Follows'  is null.");
          }
            _context.Follows.Add(follows);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFollows", new { id = follows.Id }, follows);
        }

        // DELETE: api/Follows/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFollows(int id)
        {
            if (_context.Follows == null)
            {
                return NotFound();
            }
            var follows = await _context.Follows.FindAsync(id);
            if (follows == null)
            {
                return NotFound();
            }

            _context.Follows.Remove(follows);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FollowsExists(int id)
        {
            return (_context.Follows?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
