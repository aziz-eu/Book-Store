using BookStore.DataAccess.Repository;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;

namespace BookStoreWeb.Areas.Admin.Controllers
{
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
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
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
                    obj.Product.ImageUrl = @"images\products" + fileName + extension;
                }

                if(obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                }

               
                _unitOfWork.Save();
                return RedirectToAction("Index");

            }
            return RedirectToAction("Index");
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

        #region API CALLS

        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Catagory,CoverType");
            return Json(new { data = productList });
        }
        #endregion


    }

}
 