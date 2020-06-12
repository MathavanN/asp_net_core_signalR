using AutoMapper;
using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestSingalR.Context;
using TestSingalR.Dto;
using TestSingalR.Entity;
using TestSingalR.Interfaces;
using TestSingalR.SignalR;

namespace TestSingalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;
        private readonly IQueue _queue;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ActivityController> _logger;
        public ActivityController(DataContext context, IMapper mapper, IUserAccessor userAccessor,
            IQueue queue, IHubContext<ChatHub> hubContext, ILogger<ActivityController> logger)
        {
            _context = context;
            _mapper = mapper;
            _userAccessor = userAccessor;
            _queue = queue;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetActivitiesAsync()
        {
            var activities = await _context.Activities.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<Activity>, IEnumerable<ActivityDto>>(activities));
        }

        [HttpGet("{userName}/byuser")]
        public async Task<IActionResult> GetActivitiesByUserIdAsync(string userName)
        {
            var activities = await _context.Activities.Where(u => u.AppUser.UserName == userName).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<Activity>, IEnumerable<ActivityDto>>(activities));
        }

        [HttpGet("{id}/details", Name = "ActivityByIdAsync")]
        public async Task<IActionResult> GetActivityByIdAsync(Guid id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
                return NotFound(new { Activity = "Activity not found" });

            return Ok(_mapper.Map<Activity, ActivityDto>(activity));
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> PutAsync(Guid id, ActivityStatusDto activityStatusDto)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
                return NotFound(new { Activity = "Activity not found" });

            activity.Status = string.Format("{0}% Processed", activityStatusDto.Percentage);
            if (activityStatusDto.Percentage == 100)
                activity.Result = "This is a completed activity";

            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<Activity, ActivityDto>(activity));
        }

        [HttpDelete("{id}/delete")]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
                return NotFound(new { Activity = "Activity not found" });

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateActivityDto createActivityDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetCurrentUserName());

            if (user == null)
                return NotFound(new { User = "User not found" });

            var activity = new Activity
            {
                Name = createActivityDto.Name,
                AppUserId = user.Id,
                Status = "Created",
                Result = string.Empty
            };

            if (string.IsNullOrWhiteSpace(activity.Name))
                activity.Name = "Default Activity";

            activity.Name = string.Format("{0} by user {1} at {2}", activity.Name, user.UserName, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));


            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            //_queue.QueueAsyncTask(() => PerformBackgroundJob(activity.Id));
            Task.Factory.StartNew(() => PerformBackgroundJob(activity.Id, user.UserName));

            return CreatedAtRoute("ActivityByIdAsync", new { id = activity.Id }, _mapper.Map<Activity, ActivityDto>(activity));
        }

        private async Task PerformBackgroundJob(Guid jobId, string user)
        {
            for (int i = 0; i < 101; i++)
            {
                await Task.Delay(1000);
                await _hubContext.Clients.Group(jobId.ToString()).SendAsync("ActivityProgress", i);

                if (i % 5 == 0)
                    await _hubContext.Clients.All.SendAsync("ReceiveStatus", jobId, user, i);
            }
        }
    }
}