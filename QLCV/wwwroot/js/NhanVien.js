
document.addEventListener("DOMContentLoaded", function () {
    // const rows = document.querySelectorAll("#nhanVienTable tr");
    // const itemsPerPage = 5;
    // let currentPage = 1;
    // const totalPages = Math.ceil(rows.length / itemsPerPage);
    // const paginationContainer = document.getElementById("pagination");

    // function showPage(page) {
    //     currentPage = page;
    //     const start = (page - 1) * itemsPerPage;
    //     const end = start + itemsPerPage;

    //     rows.forEach((row, index) => {
    //         row.style.display = index >= start && index < end ? "" : "none";
    //     });

    //     renderPagination();
    // }

    // function renderPagination() {
    //     paginationContainer.innerHTML = "";
    //     for (let i = 1; i <= totalPages; i++) {
    //         const btn = document.createElement("button");
    //         btn.textContent = i;
    //         btn.className = "btn btn-sm btn-outline-primary mx-1";
    //         if (i === currentPage) btn.classList.add("active");

    //         btn.addEventListener("click", () => showPage(i));
    //         paginationContainer.appendChild(btn);
    //     }
    // }

    // // Khởi tạo
    // if (rows.length > 0) {
    //     showPage(1);
    // }
});
function loadEditPhongBan() {
    $.ajax({
        url: '/NhanVien/GetPhongBan',
        type: 'GET',
        success: function (data) {
            const select = $('#IdEditPhongBan');
            select.empty();
            select.append('<option value="">-- Chọn phòng ban --</option>');

            $.each(data, function (i, item) {
                select.append(`<option value="${item.idPhongBan}">${item.tenPhongBan}</option>`);
            });

        }
    });
}
function formatDate(dateString) {
      if (!dateString) return '';
  
      
      if (dateString.includes('/')) {
          const [day, month, year] = dateString.split('/');
          return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
      }
  
      const date = new Date(dateString); // có thể là ISO hoặc timestamp
      const year = date.getFullYear();
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      const day = date.getDate().toString().padStart(2, '0');
      
      return `${year}-${month}-${day}`;
  }

$(document).ready(function () {
    $('.btnEdit').on('click', function(){
    const data = $(this).data('item');
    const phongBanId = $(this).data('phongban');
    loadEditPhongBan();
    // Nếu trình duyệt convert thành string → parse lại
    if (typeof data === 'string') {
        data = JSON.parse(data);
    }

    console.log(data); // Kiểm tra dữ liệu nhận được
     
    // Gán dữ liệu vào modal Edit
    $('#modalEditNhanVien input[name="idNhanVien"]').val(data.IdNhanVien);
    $('#modalEditNhanVien input[name="HoTen"]').val(data.HoTen);
    $('#modalEditNhanVien input[name="NgaySinh"]').val(formatDate(data.NgaySinh));
    $('#modalEditNhanVien input[name="GioiTinh"]').val(data.GioiTinh);
    $('#modalEditNhanVien input[name="SoDienThoai"]').val(data.SoDienThoai);
    $('#modalEditNhanVien input[name="Email"]').val(data.Email);
    $('#modalEditNhanVien input[name="ChucVu"]').val(data.ChucVu);
        loadEditPhongBan(data.idPhongBan);
    const $roleSelect = $('#modalEditNhanVien select[name="Account.Role"]');
    if (data.Account) {
        $('#modalEditNhanVien input[name="Account.Id"]').val(data.Account.IdAccount ?? '');
        $('#modalEditNhanVien input[name="Account.Username"]').val(data.Account.Username ?? '');
        // It's common to leave password blank to avoid showing it; here we prefill if provided:
        $('#modalEditNhanVien input[name="Account.Password"]').val(data.Account.Password ?? '');
        // set role (enum string expected)
        // robust role selection: try value, then match by text, then numeric index fallback
        const roleVal = data.Account.Role;
        if (roleVal === null || roleVal === undefined || roleVal === '') {
            $roleSelect.val('');
        } else {
            const roleStr = roleVal.toString();
            if ($roleSelect.find(`option[value="${roleStr}"]`).length) {
                $roleSelect.val(roleStr);
            } else {
                // try match by option text (case-insensitive)
                let matchedVal = '';
                $roleSelect.find('option').each(function () {
                    if ($(this).text().toLowerCase() === roleStr.toLowerCase()) {
                        matchedVal = $(this).val();
                        return false;
                    }
                });
                if (matchedVal) {
                    $roleSelect.val(matchedVal);
                } else if (!isNaN(roleVal)) {
                    // fallback: if server sent numeric enum, try using option index
                    const idx = parseInt(roleVal, 10);
                    const opt = $roleSelect.find('option').eq(idx);
                    if (opt.length) $roleSelect.val(opt.val());
                    else $roleSelect.val('');
                } else {
                    $roleSelect.val('');
                }
            }
        }
    } else {
        // clear account inputs
        $('#modalEditNhanVien input[name="Account.Id"]').val('');
        $('#modalEditNhanVien input[name="Account.Username"]').val('');
        $('#modalEditNhanVien input[name="Account.Password"]').val('');
        $('#modalEditNhanVien select[name="Account.Role"]').val('');
        $roleSelect.val('');
    }
    $('#modalEditNhanVien').modal('show');
    
    setTimeout(() => $('#IdEditPhongBan').val(phongBanId), 300);
    });
    // Delete
    $('#SoDienThoai').on('input', function () {
    // Loại bỏ ký tự không phải số
        this.value = this.value.replace(/[^0-9]/g, '');
    });
    $('.btn-delete').click(function () {
        if (confirm('Bạn có chắc chắn muốn xóa nhân viên này?')) {
            const id = $(this).data('id');
            $.post('/NhanVien/Delete', { id: id }, function () {
                location.reload();
            });
        }
    });

    // Details
    $('.btn-details').on('click', function () {
        const id = $(this).data('id');
        window.location.href = `/NhanVien/Details/${id}`;
    });

    // Search
    $('#searchInput').on('keyup', function () {
        searchTable($(this).val());
    });

    $('#search-addon').on('click', function () {
        const value = $('#searchInput').val();
        searchTable(value);
    });

    function searchTable(keyword) {
        const value = keyword.toLowerCase();

        $('#phongBanTable tr').each(function () {
            const HoTen = $(this).find('.HoTen').text().toLowerCase();
            const NgaySinh = $(this).find('.NgaySinh').text().toLowerCase();
            const GioiTinh = $(this).find('.GioiTinh').text().toLowerCase();
            const SoDienThoai = $(this).find('.SoDienThoai').text().toLowerCase();
            const Email = $(this).find('.Email').text().toLowerCase();
            const ChucVu = $(this).find('.ChucVu').text().toLowerCase();

            const match =
                HoTen.includes(value) ||
                NgaySinh.includes(value) ||
                GioiTinh.includes(value) ||
                SoDienThoai.includes(value) ||
                Email.includes(value) ||
                ChucVu.includes(value);

            $(this).toggle(match);
        });
    }
});

