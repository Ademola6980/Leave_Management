using AutoMapper;
using Leave_Management.Contracts;
using Leave_Management.Data;
using Leave_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Leave_Management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _LeaveTyperepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;
        private readonly ILeaveAllocationRepository _leaveallocationrepo;

        public LeaveRequestController
            (
            ILeaveRequestRepository leaveRequestRepo,
             IMapper mapper,
             ILeaveTypeRepository LeaveTyperepo,
             ILeaveAllocationRepository leaveallocationrepo,
             UserManager<Employee> userManager
            )
        {
            _leaveRequestRepo = leaveRequestRepo;
            _mapper = mapper;
            _userManager = userManager;
            _LeaveTyperepo = LeaveTyperepo;
            _leaveallocationrepo = leaveallocationrepo;

        }
        // GET: LeaveRequestController
        [Authorize(Roles ="Administrator")]
        public async Task<ActionResult> Index()
        {
            var leaverequest = await _leaveRequestRepo.FindAll();
            var leaverequestvm = _mapper.Map<List<LeaveRequestVM>>(leaverequest);
            var model = new AdminLeaveRequestViewVM
            {
                TotalRequest = leaverequestvm.Count,
                ApprovedRequest = leaverequestvm.Count(q => q.Approved == true),
                RejectedRequset = leaverequestvm.Count(q => q.Approved == false),
                PendingRequest = leaverequestvm.Count(q => q.Approved == null),
                LeaveRequests = leaverequestvm
            };
            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaverequest = await _leaveRequestRepo.FindById(id);
            var model= _mapper.Map<LeaveRequestVM>(leaverequest);
            return View(model);
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leavetypes =await _LeaveTyperepo.FindAll();
            var leavetypeItem = leavetypes.Select(q => new SelectListItem
            {
                Text=q.Name,
                Value=q.Id.ToString()
            });
            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leavetypeItem
            };
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {
            try
            {
                var StartDate = Convert.ToDateTime(model.StartDate);
                var EndDate = Convert.ToDateTime(model.EndDate);
                var leavetypes =await _LeaveTyperepo.FindAll();
                var leavetypeItem = leavetypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });
                model.LeaveTypes = leavetypeItem;
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                if(DateTime.Compare(StartDate,EndDate)>1)
                {
                    ModelState.AddModelError("", "Start Date can not be greater than End date!!");
                    return View(model);
                }
                var employee = await _userManager.GetUserAsync(User);
                var leaveallocation = await _leaveallocationrepo.GetLeaveAllocationByEmployeeAndType(employee.Id, model.LeaveTypeId);
                Console.WriteLine("leaveallocation:" + leaveallocation.NumberOfDays);
                Console.WriteLine("EndDate:" + EndDate);
                Console.WriteLine("StartDate:" + StartDate);
                int daysRequest = (int)(EndDate - StartDate).TotalDays;
                Console.WriteLine("daysRequest:" + daysRequest);
                if (daysRequest > leaveallocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "Number of days requested is greater than Number of days Allocated!!");
                    return View(model);
                }
                var leaveRequestmodel = new LeaveRequestVM
                {
                    RequestingEmployeeId=employee.Id,
                    LeaveTypeId=leaveallocation.LeaveTypeId,
                    StartDate=StartDate,
                    EndDate=EndDate,
                    Approved=null,
                    DateRequested=DateTime.Now,
                    DateActioned=DateTime.Now

                };
                var LeaveRequests = _mapper.Map<LeaveRequest>(leaveRequestmodel);
                var isSucess = await _leaveRequestRepo.Create(LeaveRequests);
                if(!isSucess)
                {
                    ModelState.AddModelError("", "Errors while saving data!!");
                    return View(model);
                }
                return RedirectToAction(nameof(MyLeave));
            }
            catch (Exception)
            {

                return View(model);
            }
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequests =await _leaveRequestRepo.FindById(id);
                leaveRequests.ApprovedBy = user;
                leaveRequests.ApprovedById = user.Id;
                leaveRequests.Approved = true;
                leaveRequests.DateActioned = DateTime.Now;
                var allocation = await _leaveallocationrepo.GetLeaveAllocationByEmployeeAndType(leaveRequests.RequestingEmployeeId, leaveRequests.LeaveTypeId);
                int daysRequest = (int)(leaveRequests.EndDate - leaveRequests.StartDate).TotalDays;
                Console.WriteLine("daysRequest:"+ daysRequest);
                allocation.NumberOfDays -= daysRequest;
                Console.WriteLine("allocation.NumberOfDays:" + allocation.NumberOfDays);
                await _leaveRequestRepo.Update(leaveRequests);
                 await _leaveallocationrepo.Update(allocation);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));

            }
           
        }
        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequests =await _leaveRequestRepo.FindById(id);
                leaveRequests.ApprovedBy = user;
                leaveRequests.ApprovedById = user.Id;
                leaveRequests.Approved = false;
                leaveRequests.DateActioned = DateTime.Now;

                var isSuccess =await _leaveRequestRepo.Update(leaveRequests);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                return RedirectToAction(nameof(Index));
            }
          
        }

        public async Task<ActionResult> MyLeave()
        {
            var employee = await _userManager.GetUserAsync(User);
            var employeeid=employee.Id;
            var leaveRequest = await _leaveRequestRepo.FindByEmployeeId(employeeid);
            var leaveAllocation = await _leaveallocationrepo.GetLeaveAllocationByEmployee(employeeid);
            var leaveRequestModel= _mapper.Map<List<LeaveRequestVM>>(leaveRequest); 
            var leaveAllocationModel= _mapper.Map<List<LeaveAllocationVM>>(leaveAllocation);
            var model = new MyLeaveRequestVM
            {
                LeaveAllocations = leaveAllocationModel,
                LeaveRequests=leaveRequestModel,
            };

            return View(model);
        }

        public async Task<ActionResult> CancelRequest(int id)
        {
            try
            {
                var employee = await _userManager.GetUserAsync(User);
                var LeaveRequest=await _leaveRequestRepo.FindById(id);
                var StartDate = Convert.ToDateTime(LeaveRequest.StartDate);
                var EndDate = Convert.ToDateTime(LeaveRequest.EndDate);
                Console.WriteLine("StartDate:" + StartDate);
                Console.WriteLine("EndDate" + EndDate);
                int daysRequested = (int)(StartDate - EndDate).TotalDays;
                var allocation = await _leaveallocationrepo.GetLeaveAllocationByEmployeeAndType(employee.Id, LeaveRequest.LeaveTypeId);
                Console.WriteLine("daysRequested:" + daysRequested);
                Console.WriteLine("allocation:" + allocation.NumberOfDays);
                allocation.NumberOfDays += daysRequested;
                Console.WriteLine("New allocation:" + allocation.NumberOfDays);
                LeaveRequest.CancelRequest=true;
                LeaveRequest.Approved=false;
               await _leaveRequestRepo.Update(LeaveRequest);
                 await _leaveallocationrepo.Update(allocation);

               

                return RedirectToAction(nameof(MyLeave));
            }
            catch (Exception)
            {

                return RedirectToAction(nameof(MyLeave));
            }
            
        }
        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
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
