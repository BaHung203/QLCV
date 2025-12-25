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
        public async Task<PagedResult<nhanVien>> GetAllAsync(string keyword, int page, int pageSize)
        {
            keyword = keyword?.Trim().ToLower() ?? string.Empty;

            var query = _context.nhanVien
                .Include(nv => nv.PhongBan)
                .Include(nv => nv.Account)
                .AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c =>
                    (c.HoTen ?? "").ToLower().Contains(keyword) ||
                    (c.GioiTinh ?? "").ToLower().Contains(keyword) ||
                    (c.SoDienThoai ?? "").ToLower().Contains(keyword) ||
                    (c.ChucVu ?? "").ToLower().Contains(keyword) ||
                    (c.PhongBan.TenPhongBan ?? "").ToLower().Contains(keyword)
                );
            }

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
                .Include(nv => nv.Account)
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
            var existing = await _context.nhanVien
               .Include(x => x.Account)
               .FirstOrDefaultAsync(x => x.IdNhanVien == nv.IdNhanVien);

            if (existing == null) throw new Exception("Không tìm thấy nhân viên.");

            existing.HoTen = nv.HoTen;
            existing.NgaySinh = nv.NgaySinh;
            existing.GioiTinh = nv.GioiTinh;
            existing.SoDienThoai = nv.SoDienThoai;
            existing.Email = nv.Email;
            existing.ChucVu = nv.ChucVu;
            existing.IdPhongBan = nv.IdPhongBan;

            if (nv.Account != null)
            {
                if (existing.Account == null)
                {
                    // Create new account and link to employee
                    var newAcc = new Account
                    {
                        Username = nv.Account.Username,
                        // only set password if provided
                        Password = string.IsNullOrEmpty(nv.Account.Password) ? "" : nv.Account.Password,
                        Email = nv.Account.Email,
                        Role = nv.Account.Role,
                        IdNhanVien = existing.IdNhanVien
                    };
                    _context.Accounts.Add(newAcc);
                    existing.Account = newAcc;
                }
                else
                {
                    // Update fields; only update password if non-empty (user didn't clear it)
                    existing.Account.Username = nv.Account.Username;
                    existing.Account.Email = nv.Account.Email;
                    existing.Account.Role = nv.Account.Role;
                    if (!string.IsNullOrEmpty(nv.Account.Password))
                    {
                        existing.Account.Password = nv.Account.Password;
                    }
                }
            }

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
