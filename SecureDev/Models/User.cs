using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Security.Application;
using Vladi2.Helpers;

namespace Vladi2.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PictureUrl { get; set; }
        public bool IsAdmin { get; set; }
        public int CountsAttempts { get; set; }
        public DateTime LastAttempt { get; set; }

        public UserResult Login()
        {
            this.UserName = Sanitizer.GetSafeHtmlFragment(this.UserName);
            this.Password = Sanitizer.GetSafeHtmlFragment(this.Password);

            if (String.IsNullOrEmpty(this.UserName) || String.IsNullOrEmpty(this.Password))
                return new UserResult(UserResult.Statuses.NoInformation, "Please enter user name and password!");
            try
            {
                var connectionString = string.Format("DataSource={0}", HttpContext.Current.Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();
                    using (SQLiteCommand LoginCommand = new SQLiteCommand("SELECT id,username,password,firstname,lastname,isadmin,logincounts,lastattempt,email,phone,pictureUrl FROM Users Where userName = @username", m_dbConnection))
                    {
                        LoginCommand.Parameters.Add(new SQLiteParameter("username", this.UserName));
                        using (SQLiteDataReader reader = LoginCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User myUser = new User()
                                {
                                    UserID = int.Parse(reader["id"].ToString()),
                                    UserName = reader["username"].ToString(),
                                    Password = reader["password"].ToString(),
                                    FirstName = reader["firstname"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Phone = reader["phone"].ToString(),
                                    LastName = reader["lastname"].ToString(),
                                    IsAdmin = reader["isadmin"].ToString() == "1",
                                    CountsAttempts = int.Parse(reader["logincounts"].ToString()),
                                    LastAttempt = DateTime.Parse(reader["lastattempt"].ToString()),
                                    PictureUrl = reader["pictureUrl"].ToString()
                                };

                                if (myUser.CountsAttempts < 5 || (DateTime.Now - myUser.LastAttempt).TotalMinutes >= 20)
                                {
                                    if (Sha256(this.Password) == myUser.Password) //SHA256
                                    {
                                        //clear unseccess attempts
                                        using (SQLiteCommand clearUnseccess = new SQLiteCommand("update users set logincounts = 0, lastattempt = datetime('now', 'localtime') where username = @username", m_dbConnection))
                                        {
                                            clearUnseccess.Parameters.Add(new SQLiteParameter("username", this.UserName));
                                            clearUnseccess.ExecuteNonQuery();
                                        }
                                        HttpContext.Current.Session["myUser"] = myUser;
                                        Logger.WriteToLog(Logger.LoginSuccess);
                                        return new UserResult(UserResult.Statuses.Success,"");
                                    }
                                    else
                                    {
                                        //Update Unseccessful attempts
                                        using (SQLiteCommand AttemptsCommand = new SQLiteCommand("update users set logincounts = (select logincounts +1 from users where username = @username), lastattempt = datetime('now', 'localtime') where username = @username", m_dbConnection))
                                        {
                                            AttemptsCommand.Parameters.Add(new SQLiteParameter("username", this.UserName));
                                            AttemptsCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else
                                    return new UserResult(UserResult.Statuses.UserBanned, "You got banned! Please wait 20 minutes");
                            }
                        }
                    }
                }
                return new UserResult(UserResult.Statuses.UsernameOrPasswordIsIncorrect, "User name or Password is incorrect");
            }
            catch (SQLiteException)
            {
                Logger.WriteToLog(Logger.SQLLiteMsg);
                throw;
            }
            catch (Exception exception)
            {
                Logger.WriteToLog(exception);
                throw;
            }
        }

        public UserResult ValidUser(HttpPostedFileBase file,bool isregister = false)
        {
            this.FirstName = this.FirstName == null ? "" : Sanitizer.GetSafeHtmlFragment(this.FirstName);
            this.LastName = this.LastName == null ? "" : Sanitizer.GetSafeHtmlFragment(this.LastName);
            this.UserName = this.UserName == null ? "" : Sanitizer.GetSafeHtmlFragment(this.UserName);
            this.Password = this.Password == null ? "" : Sanitizer.GetSafeHtmlFragment(this.Password);
            this.Email = this.Email == null ? "" : Sanitizer.GetSafeHtmlFragment(this.Email);
            this.Phone = this.Phone == null ? "" : Sanitizer.GetSafeHtmlFragment(this.Phone);
            string connectionString = string.Format("DataSource={0}", HttpContext.Current.Server.MapPath(@"~\Sqlite\db.sqlite"));


            Regex regex;
            Match match;

            regex = new Regex(@"^[a-zA-Z]{3,6}[_]{0,1}[a-zA-Z0-9]{0,6}$");
            match = regex.Match(this.UserName);

            if (isregister)
            {
                if (!match.Success)
                    return new UserResult(UserResult.Statuses.NotValidUserName,
                        "User Name must start with letters and be between 3 - 12 letters");
                else
                {
                    using (var m_dbConnection = new SQLiteConnection(connectionString))
                    {
                        try
                        {
                            m_dbConnection.Open();
                            using (SQLiteCommand checkUserNameCommand =new SQLiteCommand("SELECT username FROM Users Where userName = @username",m_dbConnection))
                            {
                                checkUserNameCommand.Parameters.Add(new SQLiteParameter("username", this.UserName));
                                using (SQLiteDataReader reader = checkUserNameCommand.ExecuteReader())
                                {
                                    //userName has been already taken
                                    if (reader.HasRows)
                                    {
                                        return new UserResult(UserResult.Statuses.UserNameTaken,
                                            "User Name has been already taken");
                                    }
                                }
                            }
                        }
                        catch (SQLiteException)
                        {
                            Logger.WriteToLog(Logger.SQLLiteMsg);
                            throw;
                        }
                        catch (Exception exception)
                        {
                            Logger.WriteToLog(exception);
                            throw;
                        }
                    }
                }
            }

            if (isregister)
            {
                //check at least 8 chars, at least one uppercase letter, lowercase, number
                regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,15}$");
                match = regex.Match(this.Password);

                if (!match.Success)
                {
                    return new UserResult(UserResult.Statuses.NotValidPassword,
                        "The password must be between 8 - 15 chars, at least 1 uppercase letter, 1 lowercase letter and 1 digit");
                }
            }


            regex = new Regex(@"^[a-zA-Z]{1,6}[-]{0,1}[a-zA-Z]{1,6}$");
            match = regex.Match(this.FirstName);

            if (!match.Success)
            {
                return new UserResult(UserResult.Statuses.NotValidFirstName, "First Name must be between 2 - 12 letters");
            }


            regex = new Regex(@"^[a-zA-Z]{1,8}[-]{0,1}[a-zA-Z]{1,8}$");
            match = regex.Match(this.LastName);

            if (!match.Success)
            {
                return new UserResult(UserResult.Statuses.NotValidLastName, "Last Name must be between 2 - 16 letters");
            }


            regex = new Regex(@"^(([a-zA-Z0-9._%-]{2,30})@([a-zA-Z.-]{3,12})\.([a-zA-Z]{2,5}))$");
            match = regex.Match(this.Email);

            if (!match.Success)
            {
                return new UserResult(UserResult.Statuses.NotValidEmail, "Not a valid email");
            }


            regex = new Regex(@"^[0-9]{10,10}$");
            match = regex.Match(this.Phone);

            if (!match.Success)
            {
                return new UserResult(UserResult.Statuses.NotValidPhone, "Not a valid phone number");
            }

            //upload image
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
                    return new UserResult(UserResult.Statuses.NotValidFile, "Only images are allowed");
                }
                this.PictureUrl = String.Format(@"~/ProfileImages/{0}.{1}", Guid.NewGuid().ToString(),Path.GetExtension(file.FileName).ToLower());
            }
            else
            {
                if (isregister)//no file uploaded - set default image
                    this.PictureUrl = @"~/ProfileImages/default-user.png";
                else//edit profile & no file uploaded - set the previous image
                    this.PictureUrl = (HttpContext.Current.Session["myUser"] as User).PictureUrl;
            }

            this.Password = Sha256(this.Password);
            return new UserResult(UserResult.Statuses.Success, "");
        }

        public UserResult Register(HttpPostedFileBase file)
        {
            UserResult result = ValidUser(file, true);
            if (result.Status == UserResult.Statuses.Success)
            {
                //all good
                try
                {
                    string connectionString = string.Format("DataSource={0}", HttpContext.Current.Server.MapPath(@"~\Sqlite\db.sqlite"));
                    using (var m_dbConnection = new SQLiteConnection(connectionString))
                    {
                        m_dbConnection.Open();
                        using (SQLiteCommand createUser = new SQLiteCommand("insert into users (userName, password, firstName, lastName, email, phone, pictureUrl, isAdmin, loginCounts, lastAttempt) values (@username, @password, @firstname, @lastname, @email, @phone, @pictureurl, 0, 0, datetime('now', 'localtime'))", m_dbConnection))
                        {
                            createUser.Parameters.Add(new SQLiteParameter("username", this.UserName));
                            createUser.Parameters.Add(new SQLiteParameter("password", this.Password));
                            createUser.Parameters.Add(new SQLiteParameter("pictureurl", this.PictureUrl));
                            createUser.Parameters.Add(new SQLiteParameter("firstname", this.FirstName));
                            createUser.Parameters.Add(new SQLiteParameter("lastname", this.LastName));
                            createUser.Parameters.Add(new SQLiteParameter("email", this.Email));
                            createUser.Parameters.Add(new SQLiteParameter("phone", this.Phone));
                            createUser.ExecuteNonQuery();
                        }
                        if(file!=null)
                            file.SaveAs(Path.Combine(HttpContext.Current.Server.MapPath(@"~\ProfileImages\"),Path.GetFileName(this.PictureUrl)));
                    }
                }
                catch (SQLiteException)
                {
                    Logger.WriteToLog(Logger.SQLLiteMsg);
                    throw;
                }
                catch (Exception exception)
                {
                    Logger.WriteToLog(exception);
                    throw;
                }
            }
            return result;
        }

        public UserResult UpdateProfile(HttpPostedFileBase file)
        {
            UserResult result = ValidUser(file, false);
            if (result.Status == UserResult.Statuses.Success)
            {
                //all good
                try
                {
                    string connectionString = string.Format("DataSource={0}", HttpContext.Current.Server.MapPath(@"~\Sqlite\db.sqlite"));
                    using (var m_dbConnection = new SQLiteConnection(connectionString))
                    {
                        m_dbConnection.Open();

                        using (SQLiteCommand updateUser = new SQLiteCommand("update users set firstName = @firstname, lastName = @lastname, email = @email, phone = @phone, pictureUrl = @pictureurl where userName = @username", m_dbConnection))
                        {
                            updateUser.Parameters.Add(new SQLiteParameter("username", ((User)HttpContext.Current.Session["myUser"]).UserName));
                            updateUser.Parameters.Add(new SQLiteParameter("pictureurl", this.PictureUrl));
                            updateUser.Parameters.Add(new SQLiteParameter("firstname", this.FirstName));
                            updateUser.Parameters.Add(new SQLiteParameter("lastname", this.LastName));
                            updateUser.Parameters.Add(new SQLiteParameter("email", this.Email));
                            updateUser.Parameters.Add(new SQLiteParameter("phone", this.Phone));
                            updateUser.ExecuteNonQuery();
                        }
                        if (file != null)
                            file.SaveAs(Path.Combine(HttpContext.Current.Server.MapPath(@"~\ProfileImages\"), Path.GetFileName(this.PictureUrl)));
                        ((User)HttpContext.Current.Session["myUser"]).FirstName = this.FirstName;
                        ((User)HttpContext.Current.Session["myUser"]).LastName = this.LastName;
                        ((User)HttpContext.Current.Session["myUser"]).Email = this.Email;
                        ((User)HttpContext.Current.Session["myUser"]).Phone = this.Phone;
                    }
                }
                catch (SQLiteException)
                {
                    Logger.WriteToLog(Logger.SQLLiteMsg);
                    throw;
                }
                catch (Exception exception)
                {
                    Logger.WriteToLog(exception);
                    throw;
                }
            }
            return result;
        }


        public static string Sha256(string password)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            string hash = "";
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (byte theByte in crypto)
                hash += theByte.ToString("X2");
            return hash;
        }
    }

    //this class is for returning status of login,registration and edit profile
    public class UserResult
    {
        public string Message { get; set; }
        public Statuses Status { get; set; }

        public UserResult(Statuses status, string message)
        {
            this.Status = status;
            this.Message = message;
        }

        public enum Statuses
        {
            NoInformation,
            UserBanned,
            UsernameOrPasswordIsIncorrect,
            NotValidUserName,
            UserNameTaken,
            NotValidPassword,
            NotValidFirstName,
            NotValidLastName,
            NotValidPhone,
            NotValidEmail,
            NotValidFile,
            Success
        }
    }
}