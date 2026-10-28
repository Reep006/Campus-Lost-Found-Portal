
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CampusLostFound.Models;

namespace CampusLostFound.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItemRepository repository =
            new ItemRepository();

        private readonly MatchService matchService =
            new MatchService();

        // Display and search items
        public ActionResult Index(string search, string category,
            string type)
        {
            var items = repository.GetAll(search, category);

            if (!string.IsNullOrWhiteSpace(type)
                && (type == "Lost" || type == "Found"))
            {
                items = items.Where(x => x.ItemType == type).ToList();
            }

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Type = type;

            return View(items);
        }

        // Open the Report Lost form
        public ActionResult CreateLost()
        {
            return View(new Item
            {
                ItemType = "Lost",
                ItemDate = DateTime.Today,
                Status = "Open"
            });
        }

        // Open the Report Found form
        public ActionResult CreateFound()
        {
            return View(new Item
            {
                ItemType = "Found",
                ItemDate = DateTime.Today,
                Status = "Open"
            });
        }

        // Save a lost or found report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Item item, HttpPostedFileBase image)
        {
            if (item.ItemType != "Lost" && item.ItemType != "Found")
            {
                ModelState.AddModelError(
                    "ItemType", "Invalid item type.");
            }

            if (image != null && image.ContentLength > 0)
            {
                string extension =
                    Path.GetExtension(image.FileName).ToLowerInvariant();

                string[] allowedExtensions =
                    { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "ImagePath",
                        "Use JPG, JPEG, PNG or GIF images.");
                }

                if (image.ContentLength > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "ImagePath",
                        "Image size must be 5 MB or less.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(
                    item.ItemType == "Found"
                        ? "CreateFound" : "CreateLost",
                    item);
            }

            try
            {
                item.Status = "Open";
                item.ImagePath = null;

                if (image != null && image.ContentLength > 0)
                {
                    string extension =
                        Path.GetExtension(image.FileName).ToLowerInvariant();

                    string fileName =
                        Guid.NewGuid().ToString("N") + extension;

                    string folder = Server.MapPath("~/Uploads");

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    image.SaveAs(Path.Combine(folder, fileName));
                    item.ImagePath = "/Uploads/" + fileName;
                }

                repository.Insert(item);

                TempData["Message"] =
                    "Your item report was submitted successfully.";

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("",
                    "Unable to save the report. Check the database connection.");

                return View(
                    item.ItemType == "Found"
                        ? "CreateFound" : "CreateLost",
                    item);
            }
        }

        // Display item details and potential matches
        public ActionResult Details(int id)
        {
            Item item = repository.GetById(id);

            if (item == null)
                return HttpNotFound();

            ViewBag.Matches = matchService.FindMatches(
                item, repository.GetAll());

            return View(item);
        }

        // Open the edit form (admin only)
        public ActionResult Edit(int id)
        {
            if (Session["AdminId"] == null)
                return RedirectToAction("Login", "Admin");

            Item item = repository.GetById(id);

            if (item == null)
                return HttpNotFound();

            return View(item);
        }

        // Save edited item details (admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Item item, HttpPostedFileBase image)
        {
            if (Session["AdminId"] == null)
                return RedirectToAction("Login", "Admin");

            if (!ModelState.IsValid)
                return View(item);

            Item existing = repository.GetById(item.ItemId);

            if (existing == null)
                return HttpNotFound();

            try
            {
                item.ImagePath = existing.ImagePath;

                if (image != null && image.ContentLength > 0)
                {
                    string extension =
                        Path.GetExtension(image.FileName).ToLowerInvariant();

                    string[] allowedExtensions =
                        { ".jpg", ".jpeg", ".png", ".gif" };

                    if (!allowedExtensions.Contains(extension)
                        || image.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            "ImagePath",
                            "Choose a JPG, PNG or GIF image up to 5 MB.");

                        return View(item);
                    }

                    string folder = Server.MapPath("~/Uploads");

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string fileName =
                        Guid.NewGuid().ToString("N") + extension;

                    image.SaveAs(Path.Combine(folder, fileName));
                    item.ImagePath = "/Uploads/" + fileName;
                }

                repository.Update(item);

                TempData["Message"] = "Item updated successfully.";

                return RedirectToAction("Index", "Admin");
            }
            catch (Exception)
            {
                ModelState.AddModelError("",
                    "Unable to update the item.");

                return View(item);
            }
        }

        // Delete a report (admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Session["AdminId"] == null)
                return RedirectToAction("Login", "Admin");

            try
            {
                repository.Delete(id);
                TempData["Message"] = "Item deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Message"] = "Unable to delete the item.";
            }

            return RedirectToAction("Index", "Admin");
        }

        // Mark an item as returned (admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkReturned(int id)
        {
            if (Session["AdminId"] == null)
                return RedirectToAction("Login", "Admin");

            Item item = repository.GetById(id);

            if (item == null)
                return HttpNotFound();

            item.Status = "Returned";
            repository.Update(item);

            TempData["Message"] = "Item marked as returned.";

            return RedirectToAction("Index", "Admin");
        }
    }
}
