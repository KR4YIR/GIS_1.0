using Microsoft.EntityFrameworkCore;
using WebApplication3.Entities;

namespace WebApplication3
{
    public class ApiService : IPointService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApiService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Point>> AddAsync(Point point)
        {
            point.CreatedDate = DateTime.UtcNow;
            var result = await _unitOfWork.Points.AddAsync(point);
            await _unitOfWork.SaveAsync();
            return new ApiResponse<Point>(
                result,
                result ? "point added" : "point can't be added",
                point);
        }
        

        public async Task<ApiResponse<List<Point>>> GetAsync()
        {
            return new ApiResponse<List<Point>>(
                true,
                "points retrieved successfully",
                await _unitOfWork.ReadPoints.GetAll().ToListAsync()
                );
        }

        public async Task<ApiResponse<List<Point>>> GetAllPointsAsync()
        {
            return new ApiResponse<List<Point>>(
                true,
                "points retrieved successfully",
                await _unitOfWork.ReadPoints.GetAll().ToListAsync()
            );
        }

        public async Task<ApiResponse<List<Point>>> GetPointsByUserIdAsync(Guid userId)
        {
            var points = await _unitOfWork.ReadPoints.GetAll()
                .Where(p => p.UserId == userId) // Filter by UserId
                .ToListAsync();

            return new ApiResponse<List<Point>>(
                true,
                "user-specific points retrieved successfully",
                points
            );
        }


        public async Task<ApiResponse<Point>> GetByIdAsync(long id)
        {
            var point = await _unitOfWork.ReadPoints.GetByIdAsync(id);
            if (point == null)
                return new ApiResponse<Point>(false, "point not found", null);
            return new ApiResponse<Point>(
                true,
                "point retrieved",
                point
                );
        }

        public async Task<ApiResponse<Point>> RemoveAsync(long id)
        {
            var result = await _unitOfWork.Points.RemoveAsync(id);
            await _unitOfWork.SaveAsync();
            return new ApiResponse<Point>(
                result,
                result ? "Point removed" : "Point not found",
                null
                );
        }
        public async Task<ApiResponse<Guid>> GetPointOwnerAsync(long id)
        {
            var point = await _unitOfWork.ReadPoints.GetByIdAsync(id);
            if (point == null)
            {
                return new ApiResponse<Guid>(false, "Point not found", Guid.Empty);
            }

            return new ApiResponse<Guid>(true, "Owner retrieved successfully", point.UserId);
        }

        public async Task<ApiResponse<Point>> UpdateAsync(long id, Point updatedPoint)
        {
            
            var point = await _unitOfWork.ReadPoints.GetByIdAsync(id);

            if (point == null)
            {
                return new ApiResponse<Point>(false, "Point not found", null);
            }

            // Update with WKT only
            point.Name = updatedPoint.Name;
            point.Location = updatedPoint.Location;
            point.CreatedDate = DateTime.UtcNow;
            var result = _unitOfWork.Points.Update(point);
            await _unitOfWork.SaveAsync();
            return new ApiResponse<Point>(
                result,
                result ? "Point updated successfully" : "Point update failed",
                point
            );
        }

    }
}