using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Helpers;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr(OnlyAdmin = true)]
    public class AdminCategoriesController : BaseController
    {
       public ActionResult Index()
        {
            CategoryVM cvm = new CategoryVM();

            try
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

                cvm.categories = categories;
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
            return View(cvm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult updateCategories(CategoryVM cvm)
        {
            foreach(Category cat in cvm.categories)
            {
                bool result = cat.isValidCategory();
                if (!result)
                {
                    ViewBag.catErr = "Category name must include only letters, -, & and space";

                    return View("Index",cvm);
                }
            }

            try
            {

                var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();

                    string query = "";

                    for (int i = 1; i <= cvm.categories.Count(); ++i)
                        query = query + "WHEN categoryID = @id" + i + " THEN @value" + i + " ";


                    using (SQLiteCommand updateCategory = new SQLiteCommand(@"UPDATE Category
                                                                          SET categoryName = CASE " + query + " ELSE categoryName END", m_dbConnection))
                    {
                        for (int i = 0; i < cvm.categories.Count(); ++i)
                        {
                            updateCategory.Parameters.Add(new SQLiteParameter("id" + (i + 1), cvm.categories[i].CategoryID));
                            updateCategory.Parameters.Add(new SQLiteParameter("value" + (i + 1), cvm.categories[i].CategoryName));
                        }

                        updateCategory.ExecuteNonQuery();
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

            return RedirectToAction("Index", "AdminCategories");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult addCategory(CategoryVM cvm)
        {
            bool result = cvm.newCategory.isValidCategory();

            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));

            if (!result)
            {
                ViewBag.catErr = "Category name must include only letters, -, & and space";

                cvm.categories = new List<Category>();

                try
                {
                    using (var m_dbConnection = new SQLiteConnection(connectionString))
                    {
                        m_dbConnection.Open();
                        using (SQLiteCommand getCategoriesCommand = new SQLiteCommand("SELECT categoryID, categoryName FROM Category", m_dbConnection))
                        {
                            using (SQLiteDataReader reader = getCategoriesCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    cvm.categories.Add(new Category()
                                    {
                                        CategoryID = int.Parse(reader["categoryID"].ToString()),
                                        CategoryName = reader["categoryName"].ToString()
                                    });
                                }
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

                return View("Index", cvm);
            }

            try
            {

                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();

                    using (SQLiteCommand addCategory = new SQLiteCommand("insert into Category (categoryName) values (@category)", m_dbConnection))
                    {
                        addCategory.Parameters.Add(new SQLiteParameter("category", cvm.newCategory.CategoryName));

                        addCategory.ExecuteNonQuery();
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

            return RedirectToAction("Index", "AdminCategories");
        }
    }
}