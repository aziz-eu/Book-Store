using BookStore.DataAccess.Repository;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;

namespace BookStoreWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class ProductController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {            
            return View();
        }


        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
               
           };


            if(id== 0 || id == null){
              
                return View(productVM);
            }

            else
            {
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u =>u.Id==id);
                return View(productVM);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products\");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                       if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStrem =
                        new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStrem);
                    }
                    obj.Product.ImageUrl = @"images\products\" + fileName + extension;

                }

                if(obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                    TempData["success"] = "Created Successfully";
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                    TempData["success"] = "Updated Successfully";
                }

               
                _unitOfWork.Save();
                return RedirectToAction("Index");

            }
            return RedirectToAction("Index");
        }
            

        #region API CALLS

        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Catagory,CoverType");
            return Json(new { data = productList });
        }


        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

            if(obj == null)
            {
                return Json(new { success = false , message = "Error While Deleting"});
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product Deleted Successfully" });


        }

        #endregion


    }

}
 