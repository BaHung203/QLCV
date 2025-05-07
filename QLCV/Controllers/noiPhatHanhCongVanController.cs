using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using System.Linq;
using WebApp.Data;

public class NoiPhatHanhController : Controller
{
    private AppDbContext db = new AppDbContext();

    public ActionResult Index()
    {
        return View(db.NoiPhatHanh.ToList());
    }

    public ActionResult Details(int id)
    {
        var item = db.NoiPhatHanh.Find(id);
        // if (item == null) return HttpNotFound();
        return View(item);
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(NoiPhatHanh item)
    {
        if (ModelState.IsValid)
        {
            db.NoiPhatHanh.Add(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(item);
    }

    public ActionResult Edit(int id)
    {
        var item = db.NoiPhatHanh.Find(id);
        // if (item == null) return HttpNotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(NoiPhatHanh item)
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
        var item = db.NoiPhatHanh.Find(id);
        // if (item == null) return HttpNotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        var item = db.NoiPhatHanh.Find(id);
        db.NoiPhatHanh.Remove(item);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}
