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
                item.Grade = await EvualuateGrade(item.TotalPoints);

                List.Add(item);
            }

            return List;
        }
        public async Task<int> GetNumberOfPassed(List<StudentCourseInfo> temp)
        {
            //For each item, check if grade is 6 or above
            int count = 0;
            foreach (var item in temp)
            {
                if(item.Grade >= 6)
                {
                    count++;
                }
            }
            return count;
        }
        public async Task<int> EvualuateGrade(double points)
        {
            if(points >= 95)
            {
                return 10;
            }
            else if(points >= 85)
            {
                return 9;
            }
            else if(points >= 75)
            {
                return 8;
            }
            else if(points >= 65)
            {
                return 7;
            }
            else if(points >=55)
            {
                return 6;
            }
            else
            {
                return 0;
            }   
        }
        public  async Task<double> GetTotalPoints(int courseID)
        {
            var exams = await _context.StudentExam.Where(e => e.CourseID == courseID).ToListAsync();

            var hworks = await _context.StudentHomework.Where(e => e.CourseID == courseID).ToListAsync();

            double total = 0;
            foreach (var exam in exams)
            {
                total += exam.PointsScored;
            }

            foreach (var hwork in hworks)
            {
                total += hwork.PointsScored;
            }

            return total;

        }


        public  async Task<double> GetMaximumPoints(int? courseID)
        {
            var exams = await _context.Exam.Where(e => e.CourseID == courseID).ToListAsync();
            
            var hworks = await _context.Homework.Where(e => e.CourseID == courseID).ToListAsync();

            double total = 0;
            foreach (var exam in exams)
            {
                total += exam.TotalPoints;
            }

            foreach (var hwork in hworks)
            {
                total += hwork.TotalPoints;
            }

            return total;

        }

    }
}
