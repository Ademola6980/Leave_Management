using Leave_Management.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_Management.Contracts
{
    public interface ILeaveAllocationRepository : IRepositoryBase<LeaveAllocation>
    {
        Task<bool> CheckAllocation(int leavetypeid, string employeeid);
        Task<ICollection<LeaveAllocation>> GetLeaveAllocationById(string id);
       Task<LeaveAllocation> GetLeaveAllocationByEmployeeAndType(string employeeid,int typeid);
        Task<ICollection<LeaveAllocation>> GetLeaveAllocationByEmployee(string employeeid);
    }
}
