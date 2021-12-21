using AutoMapper;
using Leave_Management.Contracts;
using Leave_Management.Data;
using Leave_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Leave_Management.Controllers
{
    [Authorize(Roles ="Administrator")]
    public class LeaveTypeController : Controller
    {
        private readonly ILeaveTypeRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LeaveTypeController(ILeaveTypeRepository repo,IMapper mapper, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;   
           _mapper = mapper;
        }
        // GET: LeaveTypeController
        public async Task<ActionResult> Index()
        {
            var leaveType = await _unitOfWork.LeaveTypes.FindAll();
            var model = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveType.ToList());
            return View(model);
        }

        // GET: LeaveTypeController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            // var isExists = await _repo.isExists(id);
            var isExists = await _unitOfWork.LeaveTypes.isExists(q=>q.Id==id);

            if (!isExists)
            {
                return NotFound();
            }
            var leavetype= await _unitOfWork.LeaveTypes.Find(q =>q.Id==id);
            var model =_mapper.Map<LeaveTypeVM>(leavetype);
            return View(model);
        }

        // GET: LeaveTypeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeVM model)
        {
             if(!ModelState.IsValid)
            {
                //ModelState.AddModelError("", "Something went wrong!!");
                return View(model);
            }
            try
            {
                var leaveType = _mapper.Map<LeaveType>(model);
                leaveType.DateCreated = DateTime.Now;
                await _unitOfWork.LeaveTypes.Create(leaveType);
                await _unitOfWork.Save();
                //if(!isSucess)
                //{
                //    ModelState.AddModelError("", "Something went wrong!!");
                //    return View(model);
                //}
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong!!");
                return View(model);
            }
        }

        // GET: LeaveTypeController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {    
            var isExists = await _unitOfWork.LeaveTypes.isExists(q=>q.Id==id);
            if (!isExists)
            {
                return NotFound();
            }
            var leavetype= await _unitOfWork.LeaveTypes.Find(q => q.Id==id);
            var model = _mapper.Map<LeaveTypeVM>(leavetype);
            return View(model);
        }

        // POST: LeaveTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LeaveTypeVM model)
        {
            if (!ModelState.IsValid)
            {

                return View(model);
             
            }
            try
            {
                var leaveType = _mapper.Map<LeaveType>(model);
                _unitOfWork.LeaveTypes.Update(leaveType);
                _unitOfWork.Save();
                //if (!isSucess)
                //{
                //    ModelState.AddModelError("", "Something went wrong!!");
                //    return View(model);
                //}
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong!!");
                return View(model);
            }
        }

        // GET: LeaveTypeController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var leaveType = await _unitOfWork.LeaveTypes.Find(q =>q.Id==id);
            if (leaveType == null)
            {
                return NotFound(ModelState);
            }
            //var isSucess =await _repo.Delete(leaveType);
             _unitOfWork.LeaveTypes.Delete(leaveType);
            //if (!isSucess)
            //{

            //    return BadRequest();
            //}
            return RedirectToAction(nameof(Index));
        }
         

        // POST: LeaveTypeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, LeaveTypeVM model)
        {
            try
            {
               return RedirectToAction(nameof(Index));
               
            }
            catch
            {
                return View(model);
            }
        }
    }
}
