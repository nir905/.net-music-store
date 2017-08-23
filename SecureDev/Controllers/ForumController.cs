using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Models;
using System.Data.SQLite;
using Microsoft.Security.Application;

namespace Vladi2.Controllers
{
    [AuthAttr]
    public class ForumController : BaseController
    {
        // GET: Forum
        public ActionResult Index()
        {
            List<Topic> topics = new List<Topic>();


            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand getTopicsCommand =
                        new SQLiteCommand(
                            @"SELECT Topic.TopicID, Topic.Title, Users.userName, Topic.PublishTime
                              FROM Topic INNER JOIN Users ON Topic.UserID = Users.id 
                              ORDER BY Topic.TopicID",
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

            return View(topics);
        }

        public ActionResult ReadTopic(int topicID)
        {

            List<Comment> comments = new List<Comment>();
            Topic topic = new Topic();

            //topic.Author = Sanitizer.GetSafeHtmlFragment(topic.Author);
            //topic.Title = Sanitizer.GetSafeHtmlFragment(topic.Title);

            
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
                            @"SELECT Users.userName, Comment.CommentText, Comment.CommentTime
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
                            Text = reader["CommentText"].ToString()
                        });
                    }
                }
            }

            topic.Comments = comments;

            //ViewBag.topicName = topic.Title;
            return View(topic);
        }

        [HttpPost]
        public ActionResult addTopic(Topic topic)
        {

            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand updateComment =
                        new SQLiteCommand(
                            @"INSERT INTO (CommentID, TopicID, UserID, CommentText, CommentTime)
                              VALUES (@commentid, @topicid, @userid, @commentText, @commentTime)",
                            m_dbConnection);

                updateComment.Parameters.Add(new SQLiteParameter("commentid", topic.Comments.Last().CommentUser));
                updateComment.Parameters.Add(new SQLiteParameter("topicid", topic.TopicID));
                updateComment.Parameters.Add(new SQLiteParameter("userid", (Session["myUser"] as User).UserID));
                updateComment.Parameters.Add(new SQLiteParameter("commentText", topic.Comments.Last().Text));
                updateComment.Parameters.Add(new SQLiteParameter("commentTime", topic.Comments.Last().CommentTime));

                updateComment.ExecuteNonQuery();
            }


            return RedirectToAction("ReadTopic", "Forum", new { topic.TopicID });
        }

    }
}