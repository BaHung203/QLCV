using WebApp.Models;
using WebApp.ModelUI;

namespace WebApp.Services
{
    public interface INhanVienService
    {
        Task<PagedResult<nhanVien>> GetAllAsync(int page, int pageSize);
        Task<nhanVien?> GetByIdAsync(int id);
        Task<List<phongBan>> GetPhongBanListAsync();
        Task CreateAsync(nhanVien nv);
        Task UpdateAsync(nhanVien nv);
        Task DeleteAsync(int id);
        Task<int> GetEmployeeCountAsync();
    }
}
