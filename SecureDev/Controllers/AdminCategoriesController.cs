using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr(OnlyAdmin = true)]
    public class AdminCategoriesController : BaseController
    {
       public ActionResult Index()
        {
            string connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));

            List<Vladi2.Models.Category> categories = new List<Models.Category>();

            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                using (SQLiteCommand getCategoriesCommand = new SQLiteCommand("SELECT categoryID, categoryName FROM Category", m_dbConnection))
                {
                    using (SQLiteDataReader reader = getCategoriesCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Models.Category()
                            {
                                CategoryID = int.Parse(reader["categoryID"].ToString()),
                                CategoryName = reader["categoryName"].ToString()
                            });
                        }
                    }
                }
            }

            return View(categories);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult updateCategories(List<Category> categories)
        {
            foreach(Category cat in categories)
            {
                bool result = cat.isValidCategory();
                if (!result)
                {
                    ViewBag.catErr = "Category name must include only letters, -, & and space";
                    return View("Index",categories);
                }
            }



            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();

                string query = "";

                for (int i = 1; i <= categories.Count(); ++i)
                    query = query + "WHEN categoryID = @id" + i + " THEN @value" + i + " ";


                using (SQLiteCommand updateCategory = new SQLiteCommand(@"UPDATE Category
                                                                          SET categoryName = CASE " + query + " ELSE categoryName END", m_dbConnection))
                {
                    for (int i = 0; i < categories.Count(); ++i)
                    {
                        updateCategory.Parameters.Add(new SQLiteParameter("id" + (i + 1), categories[i].CategoryID));
                        updateCategory.Parameters.Add(new SQLiteParameter("value" + (i + 1), categories[i].CategoryName));
                    }

                    updateCategory.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index", "AdminCategories");
        }


        //TODO: create 'add category page'
        public ActionResult addCategory()
        {
            return View();
        }
    }
}