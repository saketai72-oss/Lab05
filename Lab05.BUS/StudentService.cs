using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab05.DAL.Entities;

namespace Lab05.BUS
{
    public class StudentService
    {
        public List<Student> GetAll()
        {
            StudentModel context = new StudentModel();
            return context.Students.ToList();
        }
    
        public List<Student> GetAllHasNoMajor()
        {
            StudentModel context = new StudentModel();
            return context.Students.Where(p => p.MajorID == null).ToList();
        }
    
        public List<Student> GetAllHasNoMajor(int facultyID)
        {
            StudentModel context = new StudentModel();
            return context.Students.Where(p => p.MajorID == null && p.FacultyID == facultyID).ToList();
        }
    
        public Student FindById(string studentId)
        {
            StudentModel context = new StudentModel();
            return context.Students.FirstOrDefault(p => p.StudentID == studentId);
        }
    
        public void InsertUpdate(Student s)
        {
            StudentModel context = new StudentModel();
            context.Students.AddOrUpdate(s);
            context.SaveChanges();
        }
        public void Delete(string studentId)
        {
            using (StudentModel context = new StudentModel())
            {
                // Tìm sinh viên theo ID
                var student = context.Students.FirstOrDefault(s => s.StudentID == studentId);

                if (student != null)
                {
                    context.Students.Remove(student);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Không tìm thấy sinh viên cần xóa!");
                }
            }
        }
        public void RegisterMajor(List<string> studentIDs, int majorID)
        {
            using (StudentModel context = new StudentModel())
            {
                var studentsToUpdate = context.Students.Where(s => studentIDs.Contains(s.StudentID)).ToList();
                if (studentsToUpdate.Count != studentIDs.Count)
                {
                    throw new Exception("Một số sinh viên không tồn tại!");
                }
                foreach (var student in studentsToUpdate)
                {
                    student.MajorID = majorID;
                }
                context.SaveChanges();
            }
        }
    }
}
