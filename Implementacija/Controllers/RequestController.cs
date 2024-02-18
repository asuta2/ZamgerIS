using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ooadproject.Data;
using ooadproject.Models;
using System.Reflection;

namespace ooadproject.Controllers
{
    public class RequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Person> _userManager;

        public RequestController(ApplicationDbContext context, UserManager<Person> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<SelectListItem> GetRequestTypesList()
        {
            var enumValues = Enum.GetValues(typeof(RequestType))
                .Cast<RequestType>()
                .Select(value => new SelectListItem
                {
                    Text = value.GetType().GetMember(value.ToString()).First()
                           .GetCustomAttribute<DisplayAttribute>()
                           .GetName(),
                    Value = ((int)value).ToString()
                })
                .ToList();

            return enumValues;
        }

        public List<SelectListItem> GetRequestStatusList()
        {
            var enumValues = Enum.GetValues(typeof(RequestStatus))
                .Cast<RequestStatus>()
                .Select(value => new SelectListItem
                {
                    Text = value.GetType().GetMember(value.ToString()).First()
                           .GetCustomAttribute<DisplayAttribute>()
                           .GetName(),
                    Value = ((int)value).ToString()
                })
                .ToList();

            return enumValues;
        }

        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Index()
        {

            var Pending = await _context.Request.Include(r => r.Processor).Include(r => r.Requester)
                .Where(r => r.Status == RequestStatus.Pending)
                .OrderBy(r => r.RequestTime)
                .ToListAsync();
            var Processed = await _context.Request.Include(r => r.Processor).Include(r => r.Requester)
                .Where(r => r.Status != RequestStatus.Pending)
                .OrderByDescending(r => r.RequestTime)
                .ToListAsync();

            ViewData["PendingRequests"] = Pending;
            ViewData["ProcessedRequests"] = Processed;

            return View(Pending);
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentRequests()
        {
            var user = await _userManager.GetUserAsync(User);

            //Get all courses a student is enrolled in
            var Courses = await _context.StudentCourse.Include(sc => sc.Course).Where(sc => sc.StudentID == user.Id).ToListAsync();
            var Pending = _context.Request.Include(r => r.Processor).Include(r => r.Requester)
            .Where(r => r.Status == RequestStatus.Pending && r.RequesterID == user.Id)
            .OrderBy(r => r.RequestTime)
            .ToList();
            //Get all requests a student has made in descending order by request time
            var Processed = _context.Request.Include(r => r.Processor).Include(r => r.Requester)
                .Where(r => r.Status != RequestStatus.Pending && r.RequesterID == user.Id)
                .OrderByDescending(r => r.RequestTime)
                .ToList();

            ViewData["PendingRequests"] = Pending;
            ViewData["RequestTypes"] = GetRequestTypesList();
            ViewData["ProcessedRequests"] = Processed;
            ViewData["Courses"] = Courses;


            return View();
        }

        [HttpPost, ActionName("EditStatus")]
        public async Task<IActionResult> Edit(int id, string status)
        {
            var request = await _context.Request.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            if (Enum.TryParse(typeof(RequestStatus), status, out var parsedStatus))
            {
                request.Status = (RequestStatus)parsedStatus;
            }
            else
            {
                return BadRequest("Invalid status value");
            }

            request.ProcessorID = (await _userManager.GetUserAsync(User)).Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var Courses = await _context.StudentCourse.Include(sc => sc.Course).Where(sc => sc.StudentID == user.Id).ToListAsync();

            ViewData["RequestTypes"] = GetRequestTypesList();     
            ViewData["Courses"] = Courses;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,RequesterID,RequestTime,Type,Status,ProcessorID")] Request request)
        {
            var user = await _userManager.GetUserAsync(User);
            request.RequesterID = user.Id;
            request.RequestTime = DateTime.UtcNow;
            request.ProcessorID = null;
            request.Status = RequestStatus.Pending;

            if (ModelState.IsValid)
            {
                await _context.Request.AddAsync(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(StudentRequests));
            }

            ViewData["RequestTypes"] = GetRequestTypesList();
            ViewData["RequestStatus"] = GetRequestStatusList();
            return View(request);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            
     
            if (id == null || _context.Request == null)
            {
                return NotFound();
            }

            var request = await _context.Request.FindAsync(id);
            //Get student by requests requesterID
            var student = await _context.Student.FindAsync(request.RequesterID);
            if (request == null)
            {
                return NotFound();
            }
            ViewData["Student"] = student;
            ViewData["RequestTypes"] = GetRequestTypesList();
            ViewData["RequestStatus"] = GetRequestStatusList();
            return View(request);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,RequesterID,RequestTime,Type,Status,ProcessorID")] Request request)
        {

            request.ProcessorID = (await _userManager.GetUserAsync(User)).Id;

            if (id != request.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    _context.Update(request);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestExists(request.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(Index);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Request == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Request'  is null.");
            }
            var request = await _context.Request.FindAsync(id);
            if (request != null)
            {
                _context.Request.Remove(request);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(StudentRequests));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Request == null)
            {
                return NotFound();
            }

            var request = await _context.Request
                .Include(r => r.Processor)
                .Include(r => r.Requester)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (request == null)
            {
                return NotFound();
            }
            var student = await _context.Student.FindAsync(request.RequesterID);
            ViewData["Student"] = student;
            return View(request);
        }


        private bool RequestExists(int id)
        {
          return (_context.Request?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
