using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSV.Models
{
    public class Student
    {

        public static string HeaderRow = "{nameof(Student.StudentId)},{nameof(Student.FirstName)},{nameof(Student.LastName)},{nameof(Student.Age},{nameof(Student.DateOfBirth)},nameof(Student.ImageData)}";

        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
      
      public Boolean MyRecord { get; set; }

        private string _DateOfBirth;
        public string DateOfBirth
        {
            get { return _DateOfBirth; }
            set
            {
                _DateOfBirth = value;

                //Convert DateOfBirth to DateTime
                DateTime dtOut;
                DateTime.TryParse(_DateOfBirth, out dtOut);
                DateOfBirthDT = dtOut;
            }
        }

    
        public DateTime DateOfBirthDT { get; internal set; }
        public string ImageData { get; set; }
        public int Age { get {
                return (DateTime.Today.Year - DateOfBirthDT.Year); } }

        public string AbsoluteUrl { get; set; }
        public string Directory { get; set; }
        public string FullPathUrl
        {
            get
            {
                return AbsoluteUrl + "/" + Directory;
            }
        }

        public List<string> Exceptions { get; set; } = new List<string>();
        public void FromCSV(string csvdata)
        {
            string[] data = csvdata.Split(",", StringSplitOptions.None);

            try
            {
                StudentId = data[0];
                FirstName = data[1];
                LastName = data[2];
                DateOfBirth = data[3];
                ImageData = data[4];
                
            }
            catch (Exception e)
            {
                Exceptions.Add(e.Message);
            }

        }

        public void FromDirectory(string directory)
        {
            Directory = directory;

            if(String.IsNullOrEmpty(directory.Trim()))
            {
                return;
            }

            string[] data = directory.Trim().Split(" ", StringSplitOptions.None);

            StudentId = data[0];
            FirstName = data[1];
            LastName = data[2];
           
        }

        public string ToCSV()
        {

                if (StudentId == "200450515")
                {

                MyRecord = true;


                }
                else
                {
                MyRecord = false;
                }
            
            
            string result = $"{StudentId},{FirstName},{LastName},{Age},{DateOfBirthDT.ToShortDateString()},{MyRecord},{ImageData}";
            return result;
        }

        public override string ToString()
        {

            if (StudentId == "200450515")
            {

                MyRecord = true;


            }
            else
            {
                MyRecord = false;
            }
            string result = $"{StudentId},{FirstName},{LastName},{Age},{DateOfBirthDT.ToShortDateString()},{MyRecord},{ImageData}";
            return result;
        }

       
    }

    public class Json
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool MyRecord { get; set; }
        public string Image { get; set; }

        public void Student(Student student)
        {
            StudentId = student.StudentId;
            FirstName = student.FirstName;
            LastName = student.LastName;
            DateOfBirth = student.DateOfBirthDT;
            Age = student.Age;
            MyRecord = student.MyRecord;
            Image = student.ImageData;
        }

    }


}
