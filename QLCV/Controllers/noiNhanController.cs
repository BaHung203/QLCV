using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using System.Linq;
using WebApp.Data;

namespace WebApp.Controllers
{
    public class NoiNhanController : Controller
{
    private AppDbContext db = new AppDbContext();

    public ActionResult Index()
    {
        return View(db.NoiNhan.ToList());
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Create(NoiNhan noiNhan)
    {
        if (ModelState.IsValid)
        {
            db.NoiNhan.Add(noiNhan);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(noiNhan);
    }

    public ActionResult Edit(int id)
    {
        var item = db.NoiNhan.Find(id);
        return View(item);
    }

    [HttpPost]
    public ActionResult Edit(NoiNhan noiNhan)
    {
        if (ModelState.IsValid)
        {
            // db.Entry(noiNhan).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(noiNhan);
    }

    public ActionResult Delete(int id)
    {
        var item = db.NoiNhan.Find(id);
        db.NoiNhan.Remove(item);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}
}
