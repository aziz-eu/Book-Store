using BookStore.DataAccess.Repository;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookStoreWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class CoverTypeController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> coverType = _unitOfWork.CoverType.GetAll();


            return View(coverType);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CoverType obj)
        {
           
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(obj);
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

            var catagoryFristOrDefault = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);

            if (catagoryFristOrDefault == null)
            {
                return NotFound();
            }
            return View(catagoryFristOrDefault);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(obj);
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
            var coverTypeDelete = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (coverTypeDelete == null)
            {
                return NotFound();
            }
            return View(coverTypeDelete);
        }
        public IActionResult DeletePost(int id)
        {
            var coverTypeDelete = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

            _unitOfWork.CoverType.Remove(coverTypeDelete);
            _unitOfWork.Save();
            TempData["Success"] = "Category Delete Successfully";
            return RedirectToAction("Index");


        }
    }
}
