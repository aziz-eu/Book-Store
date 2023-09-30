

using BookStore.DataAccess.Repository;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categories = _unitOfWork.Category.GetAll();


            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
           
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Category Added Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }

            var catagoryFristOrDefault = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);

            if (catagoryFristOrDefault == null)
            {
                return NotFound();
            }
            return View(catagoryFristOrDefault);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Category Update Successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }
        public IActionResult Delete(int id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var catagoryDelete = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            if (catagoryDelete == null)
            {
                return NotFound();
            }
            return View(catagoryDelete);
        }
        public IActionResult DeletePost(int id)
        {
            var catagoryDelete = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);

            _unitOfWork.Category.Remove(catagoryDelete);
            _unitOfWork.Save();
            TempData["Success"] = "Category Delete Successfully";
            return RedirectToAction("Index");


        }
    }
}
