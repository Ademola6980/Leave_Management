using AutoMapper;
using Leave_Management.Contracts;
using Leave_Management.Data;
using Leave_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Leave_Management.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {

        private readonly ILeaveTypeRepository _leaverepo;
        private readonly ILeaveAllocationRepository _leaveallocationrepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveAllocationController(
            ILeaveAllocationRepository leaveallocationrepo,
            ILeaveTypeRepository leaverepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<Employee> userManager
            )
        {
            _leaverepo = leaverepo;
            _leaveallocationrepo = leaveallocationrepo;
            _mapper = mapper;
            _userManager = userManager;
            _unitOfWork = unitOfWork;   
        }
        // GET: LeaveAllocationController1
        public async Task<ActionResult> Index()
        {
            var leaveType = await _unitOfWork.LeaveTypes.FindAll();
            var mappedleavetype = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveType.ToList());
            var model = new CreateLeaveAllocationVM
            {
                LeaveTypes = mappedleavetype,
                NumberUpdated = 0
            };
            return View(model);
        }
        public async Task<ActionResult> SetLeave(int id)
        {
            
            var leaveType = await _unitOfWork.LeaveTypes.Find(q=>q.Id==id);
            var employee = await _userManager.GetUsersInRoleAsync("Employee");
            var period=DateTime.Now.Year;
            foreach (var emp in employee)
            {
                if (await _unitOfWork.LeaveAllocations.isExists(q=>q.LeaveTypeId==id && q.EmployeeId==emp.Id && q.Period==period))
                    continue;
                   
                var allocation = new LeaveAllocationVM
                {
                    DateCreated = DateTime.Now,
                    EmployeeId= emp.Id,
                    LeaveTypeId=id,
                    NumberOfDays=leaveType.DefaultDays,
                    Period = DateTime.Now.Year
                     
                };
                var leaveAllocation = _mapper.Map<LeaveAllocation>(allocation);
               await _unitOfWork.LeaveAllocations.Create(leaveAllocation);
                await _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));

        }

        public async Task<ActionResult> ListEmployees()
        {
            var employee =await _userManager.GetUsersInRoleAsync("Employee");
            var model=_mapper.Map<List<EmployeeVM>>(employee);
            return View(model);
        }
        // GET: LeaveAllocationController1/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee= _mapper.Map<EmployeeVM>( await _userManager.FindByIdAsync(id));
            var period = DateTime.Now.Year;
            var allocate = await _unitOfWork.LeaveAllocations.FindAll(
                 expression: q => q.EmployeeId == id && q.Period == period,
                includes: q => q.Include(x => x.LeaveType));
            var allocations = _mapper.Map<List<LeaveAllocationVM>>(allocate);
            var model = new ViewLeaveAllocationVM
            {
              
               Employee = employee,
               LeaveAllocations = allocations
            };
            return View(model);
        }

        // GET: LeaveAllocationController1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocationController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController1/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var leaveallocation = await _unitOfWork.LeaveAllocations.Find(q=>q.Id==id,
            includes: q => q.Include(x => x.Employee).Include(x => x.LeaveType)
            );
            var model= _mapper.Map<EditLeaveAllocationVM>(leaveallocation);
            return View(model);
        }

        // POST: LeaveAllocationController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationVM model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var Numrecord =await _unitOfWork.LeaveAllocations.Find(q=>q.Id==model.Id);
                Numrecord.NumberOfDays = model.NumberOfDays;
               // var leaveAllocation = _mapper.Map<LeaveAllocation>(model);
                _unitOfWork.LeaveAllocations.Update(Numrecord);
                await _unitOfWork.Save();
               
                return RedirectToAction(nameof(Details), new {id=model.EmployeeId});
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong!!");
                return View(model);
            }
        }

        // GET: LeaveAllocationController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
