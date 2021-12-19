using Leave_Management.Contracts;
using Leave_Management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_Management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(LeaveRequest entity)
        {
           await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
             _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            var leaveHistory = await _db.LeaveRequests
                .Include(q => q.RequestingEmployee)
                .Include(q => q.LeaveType)
                .Include(q =>q.ApprovedBy)
                .ToListAsync();
            return leaveHistory;
        }

        public async Task<ICollection<LeaveRequest>> FindByEmployeeId(string EmployeeId)
        {
            var leaveRequests = await _db.LeaveRequests
               .Include(q => q.RequestingEmployee)
               .Include(q => q.LeaveType)
               .Include(q => q.ApprovedBy)
               .Where(q => q.RequestingEmployeeId == EmployeeId).ToListAsync();
            return leaveRequests; 
        }

        public async Task<LeaveRequest> FindById(int id)
        {
            var leaveHistory = await _db.LeaveRequests
                .Include(q => q.RequestingEmployee)
                .Include(q => q.LeaveType)
                .Include(q => q.ApprovedBy)
                .FirstOrDefaultAsync(q => q.Id==id);
            return leaveHistory;
        }


        public async Task<bool> isExists(int id)
        {
            var exists =await _db.LeaveRequests.AnyAsync(t => t.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
