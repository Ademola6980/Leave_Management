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
        private readonly IMapper _mapper;

        public LeaveTypeController(ILeaveTypeRepository repo,IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // GET: LeaveTypeController
        public async Task<ActionResult> Index()
        {
            var leaveType = await _repo.FindAll();
            var model = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveType.ToList());
            return View(model);
        }

        // GET: LeaveTypeController/Details/5
        public async Task<ActionResult> Details(int id)
        {
             var isExists = await _repo.isExists(id);
            if (!isExists)
            {
                return NotFound();
            }
            var leavetype= await _repo.FindById(id);
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
                var isSucess = await _repo.Create(leaveType);
                if(!isSucess)
                {
                    ModelState.AddModelError("", "Something went wrong!!");
                    return View(model);
                }
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
            var isExists = await _repo.isExists(id);
            if (!isExists)
            {
                return NotFound();
            }
            var leavetype= await _repo.FindById(id);
            var model = _mapper.Map<LeaveTypeVM>(leavetype);
            return View(model);
        }

        // POST: LeaveTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveTypeVM model)
        {
            if (!ModelState.IsValid)
            {

                return View(model);
             
            }
            try
            {
                var leaveType = _mapper.Map<LeaveType>(model);
                var isSucess =await _repo.Update(leaveType);
                if (!isSucess)
                {
                    ModelState.AddModelError("", "Something went wrong!!");
                    return View(model);
                }
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
            var leaveType = await _repo.FindById(id);
            if (leaveType == null)
            {
                return NotFound(ModelState);
            }
            var isSucess =await _repo.Delete(leaveType);
            if (!isSucess)
            {

                return BadRequest();
            }
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
