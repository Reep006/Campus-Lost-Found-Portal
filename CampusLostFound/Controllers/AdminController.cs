
using System;
using System.Web.Mvc;
using CampusLostFound.Models;

namespace CampusLostFound.Controllers
{
    public class AdminController : Controller
    {
        private readonly ItemRepository repository =
            new ItemRepository();

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["AdminId"] != null)
                return RedirectToAction("Index");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Enter your username and password.";
                return View();
            }

            Admin admin = null;

            string connectionString =
                System.Configuration.ConfigurationManager
                .ConnectionStrings["CampusLostFoundConnection"]
                .ConnectionString;

            try
            {
                using (System.Data.SqlClient.SqlConnection con =
                    new System.Data.SqlClient.SqlConnection(connectionString))
                using (System.Data.SqlClient.SqlCommand cmd =
                    new System.Data.SqlClient.SqlCommand(
                        @"SELECT AdminId, Username, PasswordHash
                          FROM dbo.Admins
                          WHERE Username = @Username", con))
                {
                    cmd.Parameters.Add(
                        "@Username",
                        System.Data.SqlDbType.NVarChar, 50).Value =
                        username.Trim();

                    con.Open();

                    using (System.Data.SqlClient.SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            admin = new Admin
                            {
                                AdminId = Convert.ToInt32(reader["AdminId"]),
                                Username = Convert.ToString(reader["Username"]),
                                PasswordHash =
                                    Convert.ToString(reader["PasswordHash"])
                            };
                        }
                    }
                }

                if (admin != null &&
                    VerifyPassword(password, admin.PasswordHash))
                {
                    Session["AdminId"] = admin.AdminId;
                    Session["AdminUsername"] = admin.Username;

                    return RedirectToAction("Index");
                }

                ViewBag.Error = "Invalid username or password.";
                return View();
            }
            catch (Exception)
            {
                ViewBag.Error =
                    "Login failed. Check the database connection.";
                return View();
            }
        }

        public ActionResult Index()
        {
            if (Session["AdminId"] == null)
                return RedirectToAction("Login");

            try
            {
                var items = repository.GetAll();

                return View(items);
            }
            catch (Exception)
            {
                return Content(
                    "Unable to load items. Check your database connection.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login");
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
                return false;

            try
            {
                string[] parts = storedHash.Split('$');

                if (parts.Length != 4 || parts[0] != "PBKDF2")
                    return false;

                int iterations = int.Parse(parts[1]);
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expectedHash = Convert.FromBase64String(parts[3]);

                using (var pbkdf2 =
                    new System.Security.Cryptography.Rfc2898DeriveBytes(
                        password,
                        salt,
                        iterations,
                        System.Security.Cryptography.HashAlgorithmName.SHA256))
                {
                    byte[] actualHash = pbkdf2.GetBytes(expectedHash.Length);

                    if (actualHash.Length != expectedHash.Length)
                        return false;

                    int difference = 0;

                    for (int i = 0; i < actualHash.Length; i++)
                        difference |= actualHash[i] ^ expectedHash[i];

                    return difference == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
