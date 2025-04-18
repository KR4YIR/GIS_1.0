using WebApplication3.Entities;

namespace WebApplication3
{
    public interface IPointService
    {
        Task<ApiResponse<List<Point>>> GetAsync();
        Task<ApiResponse<Point>> AddAsync(Point point);
        Task<ApiResponse<Point>> RemoveAsync(long id);
        Task<ApiResponse<List<Point>>> GetAllPointsAsync();
        Task<ApiResponse<List<Point>>> GetPointsByUserIdAsync(Guid userId);
        Task<ApiResponse<Guid>> GetPointOwnerAsync(long id);
        Task<ApiResponse<Point>> UpdateAsync(long id, Point updatedPoint);
        Task<ApiResponse<Point>> GetByIdAsync(long id);
    }
}
