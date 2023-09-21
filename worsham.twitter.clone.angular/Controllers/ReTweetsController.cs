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
    public class ReTweetsController : ControllerBase
    {
        private readonly TwitterCloneContext _context;

        public ReTweetsController(TwitterCloneContext context)
        {
            _context = context;
        }

        // GET: api/ReTweets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReTweets>>> GetReTweets()
        {
          if (_context.ReTweets == null)
          {
              return NotFound();
          }
            return await _context.ReTweets.ToListAsync();
        }

        // GET: api/ReTweets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReTweets>> GetReTweets(int id)
        {
          if (_context.ReTweets == null)
          {
              return NotFound();
          }
            var reTweets = await _context.ReTweets.FindAsync(id);

            if (reTweets == null)
            {
                return NotFound();
            }

            return reTweets;
        }

        // PUT: api/ReTweets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReTweets(int id, ReTweets reTweets)
        {
            if (id != reTweets.Id)
            {
                return BadRequest();
            }

            _context.Entry(reTweets).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReTweetsExists(id))
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

        // POST: api/ReTweets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ReTweets>> PostReTweets(ReTweets reTweets)
        {
          if (_context.ReTweets == null)
          {
              return Problem("Entity set 'TwitterCloneContext.ReTweets'  is null.");
          }
            _context.ReTweets.Add(reTweets);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReTweets", new { id = reTweets.Id }, reTweets);
        }

        // DELETE: api/ReTweets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReTweets(int id)
        {
            if (_context.ReTweets == null)
            {
                return NotFound();
            }
            var reTweets = await _context.ReTweets.FindAsync(id);
            if (reTweets == null)
            {
                return NotFound();
            }

            _context.ReTweets.Remove(reTweets);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReTweetsExists(int id)
        {
            return (_context.ReTweets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
