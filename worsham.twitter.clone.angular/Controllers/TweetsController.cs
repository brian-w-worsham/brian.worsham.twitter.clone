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
    public class TweetsController : ControllerBase
    {
        private readonly TwitterCloneContext _context;

        public TweetsController(TwitterCloneContext context)
        {
            _context = context;
        }

        // GET: api/Tweets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tweets>>> GetTweets()
        {
          if (_context.Tweets == null)
          {
              return NotFound();
          }
            return await _context.Tweets.ToListAsync();
        }

        // GET: api/Tweets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tweets>> GetTweets(int id)
        {
          if (_context.Tweets == null)
          {
              return NotFound();
          }
            var tweets = await _context.Tweets.FindAsync(id);

            if (tweets == null)
            {
                return NotFound();
            }

            return tweets;
        }

        // PUT: api/Tweets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTweets(int id, Tweets tweets)
        {
            if (id != tweets.Id)
            {
                return BadRequest();
            }

            _context.Entry(tweets).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TweetsExists(id))
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

        // POST: api/Tweets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tweets>> PostTweets(Tweets tweets)
        {
          if (_context.Tweets == null)
          {
              return Problem("Entity set 'TwitterCloneContext.Tweets'  is null.");
          }
            _context.Tweets.Add(tweets);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTweets", new { id = tweets.Id }, tweets);
        }

        // DELETE: api/Tweets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTweets(int id)
        {
            if (_context.Tweets == null)
            {
                return NotFound();
            }
            var tweets = await _context.Tweets.FindAsync(id);
            if (tweets == null)
            {
                return NotFound();
            }

            _context.Tweets.Remove(tweets);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TweetsExists(int id)
        {
            return (_context.Tweets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
