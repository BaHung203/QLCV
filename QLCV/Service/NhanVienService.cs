using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.ModelUI;
using WebApp.Models;

namespace WebApp.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly AppDbContext _context;

        public NhanVienService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<nhanVien>> GetAllAsync(int page, int pageSize)
        {
            var query = _context.nhanVien
                .Include(nv => nv.PhongBan)
                .AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(nv => nv.IdNhanVien)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<nhanVien>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<nhanVien?> GetByIdAsync(int id)
        {
            return await _context.nhanVien
                .Include(nv => nv.PhongBan)
                .FirstOrDefaultAsync(nv => nv.IdNhanVien == id);
        }

        public async Task<List<phongBan>> GetPhongBanListAsync()
        {
            return await _context.phongBan.ToListAsync();
        }

        public async Task CreateAsync(nhanVien nv)
        {
            _context.Add(nv);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(nhanVien nv)
        {
            var existing = await _context.nhanVien.FindAsync(nv.IdNhanVien);
            if (existing == null) throw new Exception("Không tìm thấy nhân viên.");

            existing.HoTen = nv.HoTen;
            existing.NgaySinh = nv.NgaySinh;
            existing.GioiTinh = nv.GioiTinh;
            existing.SoDienThoai = nv.SoDienThoai;
            existing.Email = nv.Email;
            existing.ChucVu = nv.ChucVu;
            existing.IdPhongBan = nv.IdPhongBan;

            _context.Update(existing);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var nv = await _context.nhanVien.FindAsync(id);
            if (nv != null)
            {
                _context.nhanVien.Remove(nv);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetEmployeeCountAsync()
        {
            return await _context.nhanVien.CountAsync();
        }
    }
}
