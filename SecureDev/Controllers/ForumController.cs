﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Models;
using System.Data.SQLite;
using Microsoft.Security.Application;
using Vladi2.Helpers;

namespace Vladi2.Controllers
{
    [AuthAttr]
    public class ForumController : BaseController
    {
        // GET: Forum
        public ActionResult Index()
        {
            List<Topic> topics = new List<Topic>();

            try
            {
                var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();
                    SQLiteCommand getTopicsCommand =
                            new SQLiteCommand(
                                @"SELECT Topic.TopicID, Topic.Title, Users.userName, Topic.PublishTime
                              FROM Topic INNER JOIN Users ON Topic.UserID = Users.id 
                              ORDER BY Topic.TopicID DESC",
                                m_dbConnection);


                    using (SQLiteDataReader reader = getTopicsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topics.Add(new Topic()
                            {
                                Author = reader["userName"].ToString(),
                                Title = reader["Title"].ToString(),
                                TopicTime = DateTime.Parse(reader["PublishTime"].ToString()),
                                TopicID = int.Parse(reader["TopicID"].ToString())
                            });
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


            return View(topics);
        }

        public ActionResult ReadTopic(int topicID, int err = 0)
        {

            List<Comment> comments = new List<Comment>();
            Topic topic = new Topic();
            topic.Comments = comments;

            try
            {
                var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();

                    SQLiteCommand getTopicCommand =
                            new SQLiteCommand(
                                @"SELECT Topic.TopicID, Topic.Title, Users.userName, Topic.PublishTime
                              FROM Topic INNER JOIN Users ON Topic.UserID = Users.id
                              WHERE Topic.TopicID = @topicId",
                                m_dbConnection);


                    getTopicCommand.Parameters.Add(new SQLiteParameter("topicId", topicID));

                    using (SQLiteDataReader reader = getTopicCommand.ExecuteReader())
                    {
                        //TopicID not found
                        if (reader.HasRows == false)
                        {
                            throw new HttpException(404, "TopicID not found");
                        }

                        while (reader.Read())
                        {
                            topic.Author = reader["userName"].ToString();
                            topic.Title = reader["Title"].ToString();
                            topic.TopicTime = DateTime.Parse(reader["PublishTime"].ToString());
                            topic.TopicID = int.Parse(reader["TopicID"].ToString());
                        }
                    }


                    SQLiteCommand getCommentsCommand =
                        new SQLiteCommand(
                            @"SELECT Users.userName, Comment.CommentText, Comment.CommentTime, Users.pictureUrl
                              FROM Users INNER JOIN Comment ON Users.id = Comment.UserID
                              WHERE Comment.TopicID = @topicId
                              ORDER BY Comment.CommentTime",
                            m_dbConnection);

                    getCommentsCommand.Parameters.Add(new SQLiteParameter("topicId", topic.TopicID));

                    using (SQLiteDataReader reader = getCommentsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comments.Add(new Comment()
                            {
                                CommentTime = DateTime.Parse(reader["CommentTime"].ToString()),
                                CommentUser = reader["userName"].ToString(),
                                Text = reader["CommentText"].ToString(),
                                pictureURL = reader["pictureUrl"].ToString()
                            });
                        }
                    }
                }



                if (err == 1)
                    ViewBag.Err = "Please fill a comment";
                else
                    ViewBag.Err = null;
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

            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult addComment(Topic topic)
        {

            topic.newComment.Text = Sanitizer.GetSafeHtmlFragment(topic.newComment.Text);

            if (String.IsNullOrEmpty(topic.newComment.Text))
            {
                int err = 1;
                return RedirectToAction("ReadTopic", "Forum", new { topic.TopicID, err });
            }

            try
            {
                var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();
                    SQLiteCommand updateComment =
                            new SQLiteCommand(
                                @"INSERT INTO Comment (TopicID, UserID, CommentText, CommentTime)
                              VALUES (@topicid, @userid, @commentText, @commentTime)",
                                m_dbConnection);

                    updateComment.Parameters.Add(new SQLiteParameter("topicid", topic.TopicID));
                    updateComment.Parameters.Add(new SQLiteParameter("userid", (Session["myUser"] as User).UserID));
                    updateComment.Parameters.Add(new SQLiteParameter("commentText", topic.newComment.Text));
                    updateComment.Parameters.Add(new SQLiteParameter("commentTime", DateTime.Now));

                    updateComment.ExecuteNonQuery();
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

            return RedirectToAction("ReadTopic", "Forum", new { topic.TopicID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult addTopic(Topic topic)
        {
            topic.Title = Sanitizer.GetSafeHtmlFragment(topic.Title);
            topic.newComment.Text = Sanitizer.GetSafeHtmlFragment(topic.newComment.Text);

            if (String.IsNullOrEmpty(topic.Title))
            {
                ViewBag.Err = "Please fill a title";
                return View(topic);
            }

            if (String.IsNullOrEmpty(topic.newComment.Text))
            {
                ViewBag.Err = "Please fill a comment";
                return View(topic);
            }

            try
            {
                var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();
                    SQLiteCommand updateTopic =
                            new SQLiteCommand(
                                @"INSERT INTO Topic (UserID, Title, PublishTime)
                              VALUES (@userid, @title, @publishtime)",
                                m_dbConnection);

                    updateTopic.Parameters.Add(new SQLiteParameter("userid", (Session["myUser"] as User).UserID));
                    updateTopic.Parameters.Add(new SQLiteParameter("title", topic.Title));
                    updateTopic.Parameters.Add(new SQLiteParameter("publishtime", DateTime.Now));

                    updateTopic.ExecuteNonQuery();

                    SQLiteCommand getTopicID =
                            new SQLiteCommand(
                                @"SELECT MAX(TopicID) as lastTopic
                              FROM Topic",
                                m_dbConnection);

                    int lastTopic = int.Parse(getTopicID.ExecuteScalar().ToString());

                    SQLiteCommand updateComment =
                        new SQLiteCommand(
                            @"INSERT INTO Comment (TopicID, UserID, CommentText, CommentTime)
                              VALUES (@topicID, @userid, @commentText, @commentTime)",
                            m_dbConnection);



                    updateComment.Parameters.Add(new SQLiteParameter("topicID", lastTopic));
                    updateComment.Parameters.Add(new SQLiteParameter("userid", (Session["myUser"] as User).UserID));
                    updateComment.Parameters.Add(new SQLiteParameter("commentText", topic.newComment.Text));
                    updateComment.Parameters.Add(new SQLiteParameter("commentTime", DateTime.Now));

                    updateComment.ExecuteNonQuery();


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

            return RedirectToAction("Index", "Forum");
        }

        public ActionResult createNewTopic()
        {
            return View("addTopic", new Topic()
            {
                Title = "",
                newComment = new Comment { Text = ""}
            });
        }

    }
}