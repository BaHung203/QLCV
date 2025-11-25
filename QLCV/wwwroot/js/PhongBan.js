
document.addEventListener("DOMContentLoaded", function () {
    const rows = document.querySelectorAll("#phongBanTable tr");
    const itemsPerPage = 5;
    let currentPage = 1;
    const totalPages = Math.ceil(rows.length / itemsPerPage);
    const paginationContainer = document.getElementById("pagination");

    function showPage(page) {
        currentPage = page;
        const start = (page - 1) * itemsPerPage;
        const end = start + itemsPerPage;

        rows.forEach((row, index) => {
            row.style.display = index >= start && index < end ? "" : "none";
        });

        renderPagination();
    }

    function renderPagination() {
        paginationContainer.innerHTML = "";
        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement("button");
            btn.textContent = i;
            btn.className = "btn btn-sm btn-outline-primary mx-1";
            if (i === currentPage) btn.classList.add("active");

            btn.addEventListener("click", () => showPage(i));
            paginationContainer.appendChild(btn);
        }
    }

    // Khởi tạo
    if (rows.length > 0) {
        showPage(1);
    }
});
function loadEditNhanVien() {
    $.ajax({
        url: '/PhongBan/GetNhanVien',
        type: 'GET',
        success: function (data) {
            const select = $('#IdEditNhanVien');
            select.empty();
            select.append('<option value="">-- Chọn Nhân Viên --</option>');

            $.each(data, function (i, item) {
                select.append(`<option value="${item.HoTen}">${item.IdTruongPhong}</option>`);
            });

        }
    });
}
$(document).ready(function () {
    $('.btnEdit').on('click', function () {
        const data = $(this).data('item');
        const nhanvienId = $(this).data('nhanvien');

        loadEditNhanVien();

        $('#modalEditPhongBan input[name="IdPhongBan"]').val(data.IdPhongBan);
        $('#modalEditPhongBan input[name="TenPhongBan"]').val(data.TenPhongBan);
        $('#modalEditPhongBan input[name="IdTruongPhong"]').val(data.IdTruongPhong);
        $('#modalEditPhongBan input[name="SoDienThoai"]').val(data.SoDienThoai);

        $('#modalEditPhongBan').modal('show');
        setTimeout(() => $('#IdEditNhanVien').val(nhanvienId), 300);

    });

    

    // Delete
    $('.btn-delete').click(function () {
        if (confirm('Bạn có chắc chắn muốn xóa phòng ban này?')) {
            const id = $(this).data('id');
            $.post('/PhongBan/Delete', { id: id }, function () {
                location.reload();
            });
        }
    });
    
    // Details
    $('.btn-details').on('click', function () {
        const id = $(this).data('id');
        window.location.href = `/PhongBan/Details/${id}`;
    });

    // Search
    $('#searchInput').on('keyup', function () {
        searchTable($(this).val());
    });

    $('#search-addon').on('click', function () {
        const value = $('#searchInput').val();
        searchTable(value);
    });
    $('#soDienThoai').on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '');
    });
    function searchTable(keyword) {
        const value = keyword.toLowerCase();

        $('#phongBanTable tr').each(function () {
            const tenPhongBan = $(this).find('.tenPhongBan').text().toLowerCase();
            const truongPhong = $(this).find('.truongPhong').text().toLowerCase();
            const soDienThoai = $(this).find('.soDienThoai').text().toLowerCase();

            const match =
                tenPhongBan.includes(value) ||
                truongPhong.includes(value) ||
                soDienThoai.includes(value);

            $(this).toggle(match);
        });
    }
    $('input[name="TenPhongBan"]').on('input', function () {
        const ten = $(this).val().trim();
        const id = $('input[name="IdPhongBan"]').val() || 0; // khi thêm id=0
        
        if (ten.length === 0) return;

        $.post('/PhongBan/CheckTen', { TenPhongBan: ten, IdPhongBan: id }, function (exists) {
            if (exists === true) {
                showToast("Phòng ban đã tồn tại!", "error");
                $('input[name="TenPhongBan"]').addClass('is-invalid');
            } else {
                $('input[name="TenPhongBan"]').removeClass('is-invalid');
            }
        });
    });
    $('#btnSave').on('click', function (e) {
        if ($('input[name="TenPhongBan"]').hasClass('is-invalid')) {
            e.preventDefault();
            showToast("Không thể lưu vì tên phòng ban đã tồn tại!", "error");
            return false;
        }
    });
});

