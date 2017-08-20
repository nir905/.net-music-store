using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Security.Application;
using Vladi2.Models;

namespace Vladi2.Helpers
{
    public class UserValidation
    {
        //if isregister = true - check username and password
        public static ErrorValidation IsValidUser(User user, string connectionString, bool isregister = false)
        {
            user.FirstName = user.FirstName == null ? "" : Sanitizer.GetSafeHtmlFragment(user.FirstName);
            user.LastName = user.LastName == null ? "" : Sanitizer.GetSafeHtmlFragment(user.LastName);
            user.UserName = user.UserName == null ? "" : Sanitizer.GetSafeHtmlFragment(user.UserName);
            user.Password = user.Password == null ? "" : Sanitizer.GetSafeHtmlFragment(user.Password);
            user.Email = user.Email == null ? "" : Sanitizer.GetSafeHtmlFragment(user.Email);
            user.Phone = user.Phone == null ? "" : Sanitizer.GetSafeHtmlFragment(user.Phone);

            Regex regex;
            Match match;

            regex = new Regex(@"^[a-zA-Z]{3,6}[_]{0,1}[a-zA-Z0-9]{0,6}$");
            match = regex.Match(user.UserName);

            if (isregister)
            {
                if (!match.Success)
                {
                    return new ErrorValidation()
                    {
                        Message = "User Name must start with letters and be between 3 - 12 letters",
                        ErrorCode = ErrorValidation.Errors.NotValidUserName
                    };
                }
                else
                {
                    using (var m_dbConnection = new SQLiteConnection(connectionString))
                    {
                        m_dbConnection.Open();
                        using (
                            SQLiteCommand checkUserNameCommand =
                                new SQLiteCommand("SELECT username FROM Users Where userName = @username",
                                    m_dbConnection))
                        {
                            checkUserNameCommand.Parameters.Add(new SQLiteParameter("username", user.UserName));
                            using (SQLiteDataReader reader = checkUserNameCommand.ExecuteReader())
                            {
                                //userName has been already taken
                                if (reader.HasRows)
                                {
                                    return new ErrorValidation()
                                    {
                                        Message = "User Name has been already taken",
                                        ErrorCode = ErrorValidation.Errors.UserNameTaken
                                    };
                                }
                            }
                        }
                    }
                }
            }

            if (isregister)
            {
                //check at least 8 chars, at least one uppercase letter, lowercase, number
                regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,15}$");
                match = regex.Match(user.Password);

                if (!match.Success)
                {
                    return new ErrorValidation()
                    {
                        Message =
                            "The password must be between 8 - 15 chars, at least 1 uppercase letter, 1 lowercase letter and 1 digit",
                        ErrorCode = ErrorValidation.Errors.NotValidPassword
                    };
                }
            }




            regex = new Regex(@"^[a-zA-Z]{1,6}[-]{0,1}[a-zA-Z]{1,6}$");
            match = regex.Match(user.FirstName);

            if (!match.Success)
            {
                return new ErrorValidation()
                {
                    Message = "First Name must be between 2 - 12 letters",
                    ErrorCode = ErrorValidation.Errors.NotValidFirstName
                };
            }



            regex = new Regex(@"^[a-zA-Z]{1,8}[-]{0,1}[a-zA-Z]{1,8}$");
            match = regex.Match(user.LastName);

            if (!match.Success)
            {
                return new ErrorValidation()
                {
                    Message = "Last Name must be between 2 - 16 letters",
                    ErrorCode = ErrorValidation.Errors.NotValidLastName
                };
            }




            regex = new Regex(@"^(([a-zA-Z0-9._%-]{2,30})@([a-zA-Z.-]{3,12})\.([a-zA-Z]{2,5}))$");
            match = regex.Match(user.Email);

            if (!match.Success)
            {
                return new ErrorValidation()
                {
                    Message = "Not a valid email",
                    ErrorCode = ErrorValidation.Errors.NotValidEmail
                };
            }




            regex = new Regex(@"^[0-9]{10,10}$");
            match = regex.Match(user.Phone);

            if (!match.Success)
            {
                return new ErrorValidation()
                {
                    Message = "Not a valid phone number",
                    ErrorCode = ErrorValidation.Errors.NotValidPhone
                };
            }

            return new ErrorValidation()
            {
                ErrorCode = ErrorValidation.Errors.Success
            };
        }

        public static UploadFileValidation UploadFile(HttpPostedFileBase file)
        {
            int permittedSizeInBytes = 4000000;//4mb
            string path = "";
            if (file != null)
            {
                if (file.ContentType.ToLower() != "image/jpg" &&
                    file.ContentType.ToLower() != "image/jpeg" &&
                    file.ContentType.ToLower() != "image/pjpeg" &&
                    file.ContentType.ToLower() != "image/gif" &&
                    file.ContentType.ToLower() != "image/x-png" &&
                    file.ContentType.ToLower() != "image/png" &&
                    Path.GetExtension(file.FileName).ToLower() != ".jpg" &&
                    Path.GetExtension(file.FileName).ToLower() != ".png" &&
                    Path.GetExtension(file.FileName).ToLower() != ".gif" &&
                    Path.GetExtension(file.FileName).ToLower() != ".jpeg")
                {
                    return new UploadFileValidation()
                    {
                        Message = "Only images are allowed",
                        ErrorCode = UploadFileValidation.Errors.Error
                    };
                }

                if (file.ContentLength > permittedSizeInBytes)
                {
                    return new UploadFileValidation()
                    {
                        Message = "Image cannot be more than 4MB",
                        ErrorCode = UploadFileValidation.Errors.Error
                    };
                }

                try
                {
                    path = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLower();
                }
                catch
                {
                    return new UploadFileValidation()
                    {
                        Message = "Cannot upload image",
                        ErrorCode = UploadFileValidation.Errors.Error
                    };
                }
            }
            return new UploadFileValidation()
            {
                ErrorCode = UploadFileValidation.Errors.Success,
                Message = path
            };
        }

        public static string sha256(string password)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            string hash = "";
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (byte theByte in crypto)
                hash += theByte.ToString("X2");
            return hash;
        }
    }

    public class ErrorValidation
    {
        public string Message { get; set; }
        public Errors ErrorCode { get; set; }

        public enum Errors
        {
            NotValidUserName,
            UserNameTaken,
            NotValidPassword,
            NotValidFirstName,
            NotValidLastName,
            NotValidPhone,
            NotValidEmail,
            Success
        }
    }

    public class UploadFileValidation
    {
        public string Message { get; set; }
        public Errors ErrorCode { get; set; }

        public enum Errors
        {
            Error,
            Success
        }
    }
}