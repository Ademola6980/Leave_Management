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
        private readonly IUnitOfWork _unitOfWork;   

        public LeaveRequestController
            (
            ILeaveRequestRepository leaveRequestRepo,
             IMapper mapper,
             ILeaveTypeRepository LeaveTyperepo,
             ILeaveAllocationRepository leaveallocationrepo,
             UserManager<Employee> userManager,
             IUnitOfWork unitOfWork
            )
        {
            _leaveRequestRepo = leaveRequestRepo;
            _mapper = mapper;
            _userManager = userManager;
            _LeaveTyperepo = LeaveTyperepo;
            _leaveallocationrepo = leaveallocationrepo;
            _unitOfWork = unitOfWork;   

        }
        // GET: LeaveRequestController
        [Authorize(Roles ="Administrator")]
        public async Task<ActionResult> Index()
        {
           
            var leaverequest = await _unitOfWork.LeaveRequests.FindAll(
              includes: q => q.Include(x => x.RequestingEmployee).Include(x => x.LeaveType)
          );

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
            
            var leaverequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id,
               includes: q => q.Include(x => x.RequestingEmployee).Include(x => x.LeaveType).Include(x => x.ApprovedBy));
            var model= _mapper.Map<LeaveRequestVM>(leaverequest);
            return View(model);
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leavetypes =await _unitOfWork.LeaveTypes.FindAll();
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
                var leavetypes =await _unitOfWork.LeaveTypes.FindAll();
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
                var period = DateTime.Now.Year;
                var leaveallocation = await _unitOfWork.LeaveAllocations.Find(q=>q.EmployeeId==employee.Id && q.Period==period && q.LeaveTypeId==model.LeaveTypeId);
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
                await _unitOfWork.LeaveRequests.Create(LeaveRequests);
                await _unitOfWork.Save();

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
                var leaveRequests =await _unitOfWork.LeaveRequests.Find(q=>q.Id==id);
                leaveRequests.ApprovedBy = user;
                leaveRequests.ApprovedById = user.Id;
                leaveRequests.Approved = true;
                leaveRequests.DateActioned = DateTime.Now;
                var period = DateTime.Now.Year;
                var allocation = await _unitOfWork.LeaveAllocations.Find(q=>q.EmployeeId==leaveRequests.RequestingEmployeeId && q.LeaveTypeId==leaveRequests.LeaveTypeId && q.Period==period);
                int daysRequest = (int)(leaveRequests.EndDate - leaveRequests.StartDate).TotalDays;
                Console.WriteLine("daysRequest:"+ daysRequest);
                allocation.NumberOfDays -= daysRequest;
                Console.WriteLine("allocation.NumberOfDays:" + allocation.NumberOfDays);
                _unitOfWork.LeaveRequests.Update(leaveRequests);
                _unitOfWork.LeaveAllocations.Update(allocation);
                await _unitOfWork.Save();
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
                var leaveRequests =await _unitOfWork.LeaveRequests.Find(q=>q.Id==id);
                leaveRequests.ApprovedBy = user;
                leaveRequests.ApprovedById = user.Id;
                leaveRequests.Approved = false;
                leaveRequests.DateActioned = DateTime.Now;

                _unitOfWork.LeaveRequests.Update(leaveRequests);
                await _unitOfWork.Save();
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
            var leaveRequest = await _unitOfWork.LeaveRequests.FindAll(q=>q.RequestingEmployeeId==employeeid);
            var leaveAllocation = await _unitOfWork.LeaveAllocations.FindAll(q=>q.EmployeeId==employeeid,
                includes:q=>q.Include(x=>x.LeaveType));
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
                var LeaveRequest=await _unitOfWork.LeaveRequests.Find(q=>q.Id==id);
                var StartDate = Convert.ToDateTime(LeaveRequest.StartDate);
                var EndDate = Convert.ToDateTime(LeaveRequest.EndDate);
                Console.WriteLine("StartDate:" + StartDate);
                Console.WriteLine("EndDate" + EndDate);
                int daysRequested = (int)(StartDate - EndDate).TotalDays;
                var period = DateTime.Now.Year;
                var allocation = await _unitOfWork.LeaveAllocations.Find(q=>q.EmployeeId==employee.Id && q.LeaveTypeId==LeaveRequest.LeaveTypeId && q.Period==period);
                Console.WriteLine("daysRequested:" + daysRequested);
                Console.WriteLine("allocation:" + allocation.NumberOfDays);
                allocation.NumberOfDays += daysRequested;
                Console.WriteLine("New allocation:" + allocation.NumberOfDays);
                LeaveRequest.CancelRequest=true;
                LeaveRequest.Approved=false;
               _unitOfWork.LeaveRequests.Update(LeaveRequest);
               _unitOfWork.LeaveAllocations.Update(allocation);
                await _unitOfWork.Save();

               

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
