using Microsoft.EntityFrameworkCore;
using ooadproject.Data;

namespace ooadproject.Models
{
    public class StudentCourseManager
    {
        private readonly ApplicationDbContext _context;  
        private readonly GradesManager _gradeManager;
        public StudentCourseManager(ApplicationDbContext context) {
            _context = context;
            _gradeManager = new GradesManager(_context);
        }
        public class StudentCourseInfo
        {               
                public StudentCourseInfo() { }
                public StudentCourse student;
                public double TotalPoints;
                public int numberOfPassed;
                public int Grade;
        }

        public GradesManager Get_gradeManager()
        {
            return _gradeManager;
        }

        public async Task<List<StudentCourseInfo>> RetrieveStudentCourseInfo(int? courseID)
        {
            var AllStudentCourses = await _context.StudentCourse
                .Include(sc => sc.Student)
                .Where(sc => sc.CourseID == courseID)
                .ToListAsync();

            var exams = await _context.StudentExam
                .Where(e => e.CourseID == courseID)
                .ToListAsync();

            var hworks = await _context.StudentHomework
                .Where(e => e.CourseID == courseID)
                .ToListAsync();

            var List = new List<StudentCourseInfo>(AllStudentCourses.Count);

            foreach (var student in AllStudentCourses)
            {
                var item = new StudentCourseInfo
                {
                    student = student,
                    TotalPoints = await GetTotalPoints(student.ID)
                };
                item.Grade = EvaluateGrade(item.TotalPoints);

                List.Add(item);
            }

            return List;
        }
        public int GetNumberOfPassed(List<StudentCourseInfo> temp)
        {
            return temp.Count(item => item.Grade >= 6);
        }
        public int EvaluateGrade(double points)
        {
            if (points >= 95)
            {
                return 10;
            }
            else if (points >= 85)
            {
                return 9;
            }
            else if (points >= 75)
            {
                return 8;
            }
            else if (points >= 65)
            {
                return 7;
            }
            else if (points >= 55)
            {
                return 6;
            }
            else
            {
                return 0;
            }
        }
        public async Task<double> GetTotalPoints(int courseID)
        {
            double total = 0;

            total += await _context.StudentExam
                .Where(e => e.CourseID == courseID)
                .SumAsync(e => e.PointsScored);

            total += await _context.StudentHomework
                .Where(e => e.CourseID == courseID)
                .SumAsync(e => e.PointsScored);

            return total;
        }


        public async Task<double> GetMaximumPoints(int? courseID)
        {
            double total = 0;

            total += await _context.Exam
                .Where(e => e.CourseID == courseID)
                .SumAsync(e => e.TotalPoints);

            total += await _context.Homework
                .Where(e => e.CourseID == courseID)
                .SumAsync(e => e.TotalPoints);

            return total;
        }

    }
}
