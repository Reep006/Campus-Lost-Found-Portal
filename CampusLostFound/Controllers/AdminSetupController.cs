
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace CampusLostFound.Controllers
{
    public class AdminSetupController : Controller
    {
        private readonly string connectionString;

        public AdminSetupController()
        {
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings[
                    "CampusLostFoundConnection"];

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "Connection string 'CampusLostFoundConnection' " +
                    "was not found in the root Web.config.");
            }

            connectionString = settings.ConnectionString;
        }

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.Admins", con))
                {
                    con.Open();

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        return Content(
                            "An admin account already exists. " +
                            "Please open /Admin/Login.");
                    }
                }

                return View();
            }
            catch (Exception)
            {
                return Content(
                    "Unable to check dbo.Admins. Verify that " +
                    "CampusLostFoundDB and the Admins table exist.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            string username,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Please fill in all fields.";
                return View();
            }

            username = username.Trim();

            if (username.Length > 50)
            {
                ViewBag.Error =
                    "Username must be 50 characters or fewer.";
                return View();
            }

            if (password.Length < 8)
            {
                ViewBag.Error =
                    "Password must contain at least 8 characters.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlTransaction transaction =
                        con.BeginTransaction(
                            IsolationLevel.Serializable))
                    {
                        try
                        {
                            using (SqlCommand check = new SqlCommand(
                                "SELECT COUNT(*) FROM dbo.Admins",
                                con, transaction))
                            {
                                if (Convert.ToInt32(
                                    check.ExecuteScalar()) > 0)
                                {
                                    transaction.Rollback();
                                    ViewBag.Error =
                                        "An admin account already exists.";
                                    return View();
                                }
                            }

                            string hash = CreatePasswordHash(password);

                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO dbo.Admins
                                  (Username, PasswordHash)
                                  VALUES (@Username, @PasswordHash)",
                                con, transaction))
                            {
                                cmd.Parameters.Add(
                                    "@Username", SqlDbType.NVarChar, 50)
                                    .Value = username;

                                cmd.Parameters.Add(
                                    "@PasswordHash", SqlDbType.NVarChar, 255)
                                    .Value = hash;

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            try { transaction.Rollback(); }
                            catch { }

                            throw;
                        }
                    }
                }

                return RedirectToAction("Login", "Admin");
            }
            catch (Exception)
            {
                ViewBag.Error =
                    "Unable to create the account. Check the database " +
                    "connection and whether the username already exists.";

                return View();
            }
        }

        private string CreatePasswordHash(string password)
        {
            const int iterations = 100000;
            byte[] salt = new byte[16];

            using (RandomNumberGenerator random =
                RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            using (Rfc2898DeriveBytes pbkdf2 =
                new Rfc2898DeriveBytes(
                    password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return "PBKDF2$" + iterations + "$" +
                    Convert.ToBase64String(salt) + "$" +
                    Convert.ToBase64String(hash);
            }
        }
    }
}
