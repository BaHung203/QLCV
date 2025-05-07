using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using System.Linq;
using WebApp.Data;

public class PhanLoaiCongVanController : Controller
{
    private AppDbContext db = new AppDbContext();

    public ActionResult Index()
    {
        return View(db.PhanLoaiCongVan.ToList());
    }

    public ActionResult Details(int id)
    {
        var item = db.PhanLoaiCongVan.Find(id);
        // if (item == null) return HttpNotFound();
        return View(item);
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(PhanLoaiCongVan item)
    {
        if (ModelState.IsValid)
        {
            db.PhanLoaiCongVan.Add(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(item);
    }

    public ActionResult Edit(int id)
    {
        var item = db.PhanLoaiCongVan.Find(id);
        // if (item == null) return HttpNotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(PhanLoaiCongVan item)
    {
        if (ModelState.IsValid)
        {
            // db.Entry(item).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(item);
    }

    public ActionResult Delete(int id)
    {
        var item = db.PhanLoaiCongVan.Find(id);
        // if (item == null) return HttpNotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        var item = db.PhanLoaiCongVan.Find(id);
        db.PhanLoaiCongVan.Remove(item);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}
