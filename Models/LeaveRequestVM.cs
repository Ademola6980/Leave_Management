using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Leave_Management.Models
{
    public class LeaveRequestVM
    {
       
        public int Id { get; set; }
        public EmployeeVM RequestingEmployee { get; set; }
        [Display(Name = "Employee name")]
        public string RequestingEmployeeId { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        public LeaveTypeVM LeaveType { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime DateRequested { get; set; }
        public DateTime DateActioned { get; set; }
        public bool? Approved { get; set; }
        public EmployeeVM ApprovedBy { get; set; }
        public string ApprovedById { get; set; }
        public bool? CancelRequest { get; set; }
    }
    public class AdminLeaveRequestViewVM
    {
        [Display(Name = "Total request")]
        public int TotalRequest { get; set; }
        [Display(Name = "Approved request")]
        public int ApprovedRequest { get; set; }
        [Display(Name = "Pending request")]
        public int PendingRequest { get; set; }
        [Display(Name = "Rejected request")]
        public int RejectedRequset { get; set; }
        public List<LeaveRequestVM> LeaveRequests { get; set; }

    }
    [Keyless]
    public class CreateLeaveRequestVM
    {
        [Display(Name = "Start Date")]
        [Required]
        public string StartDate { get; set; }
        [Display(Name = "End Date")]
        [Required]
        public string EndDate { get; set; }
        public IEnumerable<SelectListItem> LeaveTypes { get; set; }
        [Display(Name ="Leave type")]
        public int LeaveTypeId { get; set; }
    }
    public class MyLeaveRequestVM
    {
        public List<LeaveRequestVM> LeaveRequests { get; set; }
        public List<LeaveAllocationVM> LeaveAllocations { get; set; }
        public bool? CancelRequest { get; set; }
    }
  }
