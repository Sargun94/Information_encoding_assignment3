using CSV.Models;
using CSV.Models.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace CSV
{
    class Program
    {
        static void Main(string[] args)
        {

            List<string> directories = new List<string>();
            directories = FTP.GetDirectory(Constants.FTP.BaseUrl);
          
            List<Student> list_new = new List<Student>();
          
            foreach (var directory in directories)
            {
                Console.WriteLine("Directory: " + directory);
            }

                foreach (var directory in directories)
            {
                Student student = new Student() { AbsoluteUrl = Constants.FTP.BaseUrl };
                student.FromDirectory(directory);

                string infoFilePath = student.FullPathUrl + "/" + Constants.Locations.InfoFile;

                bool fileExists = FTP.FileExists(infoFilePath);
                if (fileExists == true)
                {
                    
                    Console.WriteLine("Found info file:");

                    byte[] bytes = FTP.DownloadFileData(infoFilePath);
                   
                    string csvData = Encoding.Default.GetString(bytes);

                    string[] csvlines = csvData.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                    if (csvlines.Length != 2)
                    {
                        Console.WriteLine("Error in CSV format.");
                    }
                    else
                    {
                        student.FromCSV(csvlines[1]);
                    }

                }
                else
                {
                  
                    Console.WriteLine("Could not find info file:");
                   
                }

                Console.WriteLine("Info File Path::: " + infoFilePath);

                string imageFilePath = student.FullPathUrl + "/" + Constants.Locations.ImageFile;

                bool imageFileExists = FTP.FileExists(imageFilePath);

                if (imageFileExists == true)
                {

                    Console.WriteLine("Found image file:");

                   Console.WriteLine("Image File Path " + imageFilePath);

                }
                else
                {
                   
                    Console.WriteLine("Could not find image file:");

                }

                Console.WriteLine("Image File Path::: " + imageFilePath);

                list_new.Add(student);
                
            }

            
                 List<Json> jsons = new List<Json>();
          
            using (StreamWriter fs = new StreamWriter(Constants.Locations.StudentCSVFile))
            {
                
                fs.WriteLine((nameof(Student.StudentId)) + ',' + (nameof(Student.FirstName)) + ',' + (nameof(Student.LastName)) + ',' + (nameof(Student.Age)) + ',' + (nameof(Student.DateOfBirth)) + ',' + (nameof(Student.MyRecord)) + ',' + (nameof(Student.ImageData)));
                foreach (var student in list_new)
                {
                    fs.WriteLine(student.ToCSV());
                    Console.WriteLine("CSV :: " + student.ToCSV());
                    Console.WriteLine("String :: " + student.ToString());

                    Json model = new Json();
                    model.Student(student);
                    jsons.Add(model);        
                }
            }


            string json = System.Text.Json.JsonSerializer.Serialize(jsons);
            File.WriteAllText(Models.Constants.Locations.StudentJSONFile, json);


            string[] source = File.ReadAllLines(Constants.Locations.StudentCSVFile);
            source = source.Skip(1).ToArray();


            XElement xElement = new XElement("Root",
                from str in source
                let fields = str.Split(',')
                select new XElement("Students",
                  new XAttribute("StudentID", fields[0]),
               
                    new XAttribute("FirstName", fields[1]),
                    new XElement("LastName", fields[2]),
                    new XElement("Age", fields[3]),
                    new XElement("DateOfBirth", fields[4]),
                    new XElement("ImageData", fields[5])
                   

                    )
                
            );
            Console.WriteLine(xElement);
            xElement.Save(Constants.Locations.StudentXMLFile);
            Console.WriteLine("Total Item in List Count: " + list_new.Count());

            int count_startswith = 0;

            foreach (var list in list_new)
            {
             

                if (list.FirstName.StartsWith("S"))
                {
                    count_startswith++;
                    Console.WriteLine("Starts With S>>: " + list);

                }
            }

            Console.WriteLine("Count Starts With S>>: " + count_startswith);

            //Find my record

            Student meUsingFind = list_new.Find(x => x.StudentId == "200450515");
            Console.WriteLine("My Record : " + meUsingFind);


            //Min,Average,Max age
            var average_age = list_new.Average(x => x.Age);
            var minimum_age = list_new.Min(x => x.Age);
            var maximum_age = list_new.Max(x => x.Age);

            Console.WriteLine("Average Age: " + average_age);
            Console.WriteLine("Minimum Age:" + minimum_age);
            Console.WriteLine("Maximum Age:" + maximum_age);

            UploadFile(Constants.Locations.StudentCSVFile, Constants.FTP.CSVUploadLocation);
            UploadFile(Constants.Locations.StudentJSONFile, Constants.FTP.JSONUploadLocation);
            UploadFile(Constants.Locations.StudentXMLFile, Constants.FTP.XMLUploadLocation);
          
            return;

        }
        public static string UploadFile(string sourceFilePath, string destinationFileUrl, string username = Constants.FTP.Username, string password = Constants.FTP.Password)
        {
            string output;

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(destinationFileUrl);

            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(username, password);

            // Copy the contents of the file to the request stream.
            byte[] fileContents;
            using (StreamReader sourceStream = new StreamReader(sourceFilePath))
            {
                fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            }

            //Get the length or size of the file
            request.ContentLength = fileContents.Length;

            //Write the file to the stream on the server
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            //Send the request
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                output = $"Upload File Complete, status {response.StatusDescription}";
            }
            Thread.Sleep(Constants.FTP.OperationPauseTime);

            return (output);
        }



        /// <summary>
        /// Downloads a file from an FTP site
        /// </summary>
        /// <param name="sourceFileUrl">Remote file Url</param>
        /// <param name="destinationFilePath">Destination file path</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Result of file download</returns>
        public static string DownloadFile(string sourceFileUrl, string destinationFilePath, string username = Constants.FTP.Username, string password = Constants.FTP.Password)
        {
            string output;

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(sourceFileUrl);

            //Specify the method of transaction
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(username, password);

            //Indicate Binary so that any file type can be downloaded
            request.UseBinary = true;

            try
            {
                //Create an instance of a Response object
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    //Request a Response from the server
                    using (Stream stream = response.GetResponseStream())
                    {
                        //Build a variable to hold the data using a size of 1Mb or 1024 bytes
                        byte[] buffer = new byte[1024]; //1 Mb chucks

                        //Establish a file stream to collect data from the response
                        using (FileStream fs = new FileStream(destinationFilePath, FileMode.Create))
                        {
                            //Read data from the stream at the rate of the size of the buffer
                            int ReadCount = stream.Read(buffer, 0, buffer.Length);

                            //Loop until the stream data is complete
                            while (ReadCount > 0)
                            {
                                //Write the data to the file
                                fs.Write(buffer, 0, ReadCount);

                                //Read data from the stream at the rate of the size of the buffer
                                ReadCount = stream.Read(buffer, 0, buffer.Length);
                            }
                        }
                    }

                    //Output the results to the return string
                    output = $"Download Complete, status {response.StatusDescription}";
                }

            }
            catch (Exception e)
            {
                //Something went wrong
                output = e.Message;
            }

            Thread.Sleep(Constants.FTP.OperationPauseTime);

            //Return the output of the Responce
            return (output);
        }

        

        }
       
}