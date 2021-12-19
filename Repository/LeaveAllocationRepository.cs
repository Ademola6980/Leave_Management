using Leave_Management.Contracts;
using Leave_Management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_Management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> CheckAllocation(int leavetypeid, string employeeid)
        {
            var period = DateTime.Now.Year;
            var allocation = await FindAll();
            return allocation.Where(q => q.LeaveTypeId == leavetypeid && q.EmployeeId == employeeid && q.Period == period).Any();

           
        }


        public async Task<bool> Create(LeaveAllocation entity)
        {
           await _db.LeaveAllocations.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveAllocation>> FindAll()
        {
            var leaveAllocation =await _db.LeaveAllocations
                .Include(q => q.LeaveType)
                .Include(q => q.Employee)
                .ToListAsync();
            return leaveAllocation;
        }

        public async Task<LeaveAllocation> FindById(int id)
        {
            var leaveAllocation =await _db.LeaveAllocations
                .Include(q => q.LeaveType)
                .Include(q => q.Employee)
                .FirstOrDefaultAsync(q => q.Id==id);
            return leaveAllocation;
        }

        public async Task<ICollection<LeaveAllocation>> GetLeaveAllocationByEmployee(string employeeid)
        {
            var period = DateTime.Now.Year;
            var allocation =await FindAll();
            return allocation.
                Where(q => q.EmployeeId == employeeid && q.Period == period).ToList();
        }

        public async Task<LeaveAllocation> GetLeaveAllocationByEmployeeAndType(string employeeid, int typeid)
        {
            var period = DateTime.Now.Year;
            var allocation = await FindAll();
            return allocation.
                FirstOrDefault(q => q.EmployeeId == employeeid && q.Period == period && q.LeaveTypeId==typeid);
        }

        public async Task<ICollection<LeaveAllocation>> GetLeaveAllocationById(string id)
        {
            var period = DateTime.Now.Year;
            var allocations = await FindAll();
            return allocations.
                Where(q=>q.EmployeeId==id && q.Period==period).ToList();
        }

        public async Task<bool> isExists(int id)
        {
            var exists =await _db.LeaveAllocations.AnyAsync(t => t.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            var changes =await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return await Save();
        }
    }
}
