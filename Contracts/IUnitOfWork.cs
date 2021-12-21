using Leave_Management.Data;
using System;
using System.Threading.Tasks;

namespace Leave_Management.Contracts
{
    public interface IUnitOfWork: IDisposable
    {
        IGenericRepository<LeaveType> LeaveTypes { get; }
        IGenericRepository<LeaveRequest> LeaveRequests { get; }
        IGenericRepository<LeaveAllocation> LeaveAllocations { get; }
        Task Save();
    }
}
