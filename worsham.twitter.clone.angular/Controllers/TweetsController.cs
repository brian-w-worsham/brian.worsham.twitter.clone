using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;
using IAuthorizationService = worsham.twitter.clone.angular.Services.IAuthorizationService;

namespace worsham.twitter.clone.angular.Controllers
{
    [EnableCors("AllowOrigin")]
    [ApiController]
    [Route("api/[controller]")]
    public class TweetsController : TwitterController
    {
        private readonly TwitterCloneContext _context;
        private int? _currentUserId;
        private readonly IConfiguration _configuration;

        public TweetsController(TwitterCloneContext context, ILogger<TweetsController> logger, IAuthorizationService authorizationService, IConfiguration configuration) : base(logger, authorizationService)
        {
            _context = context;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            base.OnActionExecuting(context);
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

        /// <summary>
        /// Handles the HTTP POST request to create a new tweet.
        /// </summary>
        /// <param name="postModel">The data model containing the content of the new tweet.</param>
        /// <returns>
        /// If the ModelState is valid, adds a new tweet to the database and redirects to the tweets
        /// feed page. If the ModelState is invalid, logs the validation errors, and redirects to
        /// the tweets feed page.
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PostModel postModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    

                    // Retrieve the JWT token from the Authorization header
                    var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                    var token = authorizationHeader?.Split(' ').Last();

                    // retrieve SecretKeyForJwtToken from secrets.json
                    var secretKey = _configuration["SecretKeyForJwtToken"];
                    var key = Encoding.ASCII.GetBytes(secretKey);

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    SecurityToken validatedToken;
                    var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                    // Validate the JWT token and extract the user's claims
                    // var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
                    // Todo: figure out why it fails to find the NameIdentifier claim
                    var userIdClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    // var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    int userId = int.Parse(userIdClaim.Value);
                    var user = await _context.Users.FindAsync(userId);

                    _ = _context.Add(new Tweets()
                    {
                        Content = postModel.Content,
                        CreationDateTime = DateTime.UtcNow,
                        TweeterId = userId
                    });
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
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

                    return Json(
                            new
                            {
                                success = false,
                                errorMessage = "An error occurred, and we were not able to process your tweet."
                            }
                        );
                }
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Invalid JWT token.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new tweet.");
                return Json(
                            new
                            {
                                success = false,
                                errorMessage = "An error occurred, and we were not able to process your tweet."
                            }
                        );
            }
        }
    }
}
