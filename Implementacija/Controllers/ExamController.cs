using MessagePack.Formatters;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ooadproject.Data;
using ooadproject.Models;
using System.ComponentModel.DataAnnotations;

namespace ooadproject.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Person> _userManager;
        private readonly NotificationManager _notificationManager;

        public ExamController(ApplicationDbContext context, UserManager<Person> userManager, NotificationManager notificationManager)
        {
            _context = context;
            _userManager = userManager;
            _notificationManager = notificationManager;
        }

        public List<SelectListItem> GetExamTypesList()
        {
            List<SelectListItem> Types = new List<SelectListItem>();
            var EnumValues = Enum.GetValues(typeof(ExamType));

            foreach (var value in EnumValues)
            {
                var displayAttribute = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                Types.Add(new SelectListItem
                {
                    Text = displayAttribute?.Name ?? value.ToString(),
                    Value = ((int)value).ToString()
                });
            }

            return Types;

        }
        public async Task<List<SelectListItem>> GetCourseNamesList()
        {
            var user = await _userManager.GetUserAsync(User);

            var Courses = new List<SelectListItem>();

            var UserCourses = await _context.Course.Where(c => c.TeacherID == user.Id).ToListAsync();
            foreach (var item in UserCourses)
            {
                Courses.Add(new SelectListItem() { Text = item.Name, Value = item.ID.ToString() });
            }

            return Courses;

        }
        public int GetExamRegistrations(int id)
        {
            return _context.ExamRegistration.Where(er => er.ExamID == id).CountAsync().Result;

        }
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Exam.Include(e => e.Course);
            var user = await _userManager.GetUserAsync(User);
            var courses = await _context.Course.Where(c => c.TeacherID == user.Id).ToListAsync();
            var exams = await _context.Exam.Include(e => e.Course).Where(e => courses.Contains(e.Course)).ToListAsync();
            var registered = new Dictionary<int, int>();
            foreach (var item in exams)
            {
                registered.Add(item.ID, GetExamRegistrations(item.ID));
            }

            ViewData["Courses"] = courses;
            ViewData["CoursesSelectList"] = courses.Select(c => new SelectListItem { Value = c.ID.ToString(), Text = c.Name }).ToList();
            ViewData["RegisteredForExam"] = registered;
            ViewData["ExamTypes"] = GetExamTypesList();
            ViewData["RequestVerificationToken"] = HttpContext.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(HttpContext).RequestToken;


            var viewModel = new ExamViewModel
            {
                ExamList = exams
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Exam == null)
            {
                return NotFound();
            }

            var exam = await _context.Exam
                .Include(e => e.Course)
                .FirstOrDefaultAsync(m => m.ID == id);
            var user = await _userManager.GetUserAsync(User);
            ViewData["Courses"] = await _context.Course.Where(c => c.TeacherID == user.Id).ToListAsync();
            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }


        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["CourseID"] = await GetCourseNamesList();
            ViewData["ExamTypes"] = GetExamTypesList();
            ViewData["Courses"] = await _context.Course.Where(c => c.TeacherID == user.Id).ToListAsync();
            //  ViewData["CourseNames"] = ;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CourseID,Time,Type,TotalPoints,MinimumPoints")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                await _context.AddAsync(exam);
                await _context.SaveChangesAsync();
                //exam.Attach(_notificationManager);
                //await exam.Notify();
                return Ok(exam);
            }
            ViewData["CourseID"] = new SelectList(_context.Course, "ID", "ID", exam.CourseID);
            var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            return BadRequest(new { error = error });
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Exam == null)
            {
                return NotFound();
            }

            var exam = await _context.Exam.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            ViewData["Courses"] = await _context.Course.Where(c => c.TeacherID == user.Id).ToListAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _context.Exam.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            _context.Exam.Remove(exam);
            await _context.SaveChangesAsync();

            return Ok();
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Exam == null)
            {
                return NotFound();
            }

            var exam = await _context.Exam.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            ViewData["CourseID"] = new SelectList(await _context.Course.ToListAsync(), "ID", "Name", exam.CourseID);
            var user = await _userManager.GetUserAsync(User);
            ViewData["Courses"] = await _context.Course.Where(c => c.TeacherID == user.Id).ToListAsync();
            ViewData["ExamTypes"] = GetExamTypesList();
            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CourseID,Time,Type,TotalPoints,MinimumPoints")] Exam exam)
        {
            if (id != exam.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exam);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamExists(id))
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
            ViewData["CourseID"] = new SelectList(await _context.Course.ToListAsync(), "ID", "Name", exam.CourseID);
            ViewData["Courses"] = await _context.Course.ToListAsync();
            ViewData["ExamTypes"] = GetExamTypesList();
            return View(exam);
        }


        private bool ExamExists(int id)
        {
            return (_context.Exam?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
