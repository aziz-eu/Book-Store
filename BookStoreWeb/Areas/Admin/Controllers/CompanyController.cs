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

    [Area("Admin")]
    public class CompanyController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;
       

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
       
        }


        public IActionResult Index()
        {            
            return View();
        }


        public IActionResult Upsert(int? id)
        {

            Company company = new();


            if(id== 0 || id == null){
              
                return View(company);
            }

            else
            {
                company = _unitOfWork.Company.GetFirstOrDefault(u =>u.Id==id);
                return View(company);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
               

                if(obj.Id == 0)
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["success"] = "Created Successfully";
                }
                else
                {
                    _unitOfWork.Company.Update(obj);
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
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }


        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);

            if(obj == null)
            {
                return Json(new { success = false , message = "Error While Deleting"});
            }

           

            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product Deleted Successfully" });


        }

        #endregion


    }

}
 