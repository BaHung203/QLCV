document.addEventListener("DOMContentLoaded", function () {
    const rows = document.querySelectorAll(".congVanTable tr");
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
    showPage(1);
  });
$(document).ready(function() {
    $('.btnEdit').on('click', function () {
        const btn = $(this);
        var id = btn.data('id');
        var data = btn.data('item');
        console.log(data);
        // Hiển thị modal trước
        $('#modalEditCongVan').modal("show");
    
        // Đợi modal hiển thị xong rồi set giá trị
        $('#modalEditCongVan').one('shown.bs.modal', function () {
          $('#modalEditCongVan input[name="Id"]').val(data.ID);
          $('#modalEditCongVan input[name="SoHieu"]').val(data.SoHieu);
          $('#modalEditCongVan input[name="NgayDi"]').val(formatDate(data.NgayDi));
          $('#modalEditCongVan select[name="IdNoiNhan"]').val(data.IdNoiNhan).trigger('change');
          $('#modalEditCongVan select[name="IdLoaiCongVan"]').val(data.IdLoaiCongVan).trigger('change');
          $('#modalEditCongVan input[name="Vitri"]').val(data.Vitri);
          $('#modalEditCongVan textarea[name="NoiDung"]').val(data.NoiDung);
          $('#fileNameDisplay').text(data.TepDinhKem ? `Đã đính kèm: ${data.TepDinhKem}` : '');
          $('#modalEditCongVan textarea[name="GhiChuTep"]').val(data.NoiDungTep);

          const fileNameText = data.TepDinhKem ? `Đã đính kèm: ${data.TepDinhKem}` : '';
          $('#fileNameDisplay').text(fileNameText);
      });
    });
    
    // Hàm chuyển đổi định dạng ngày (nếu cần)
    function formatDate(dateString) {
      if (!dateString) return '';
  
      // Nếu là định dạng dd/mm/yyyy
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
  
    // Xóa
    $('.btn-delete').click(function () {
        if (confirm('Bạn có chắc chắn muốn xóa ?')) {
            const id = $(this).data('id');
            $.post('/congVanDi/Delete', { id: id }, function () {
                location.reload();
            });
        }
    });
    // tìm kiếm
    $('#searchInput').on('keyup', function () {
        const value = $(this).val().toLowerCase();
        $('#congVanTable tbody tr').filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });
});
$('.btn-Details').on('click', function () {
    const id = $(this).data('id');
    window.location.href = `/CongVanDi/Details/${id}`;
});

$('#searchInput').on('keyup', function () {
    searchTable($(this).val());
  });

  // Khi bấm vào icon tìm kiếm
  $('#search-addon').on('click', function () {
    const value = $('#searchInput').val();
    searchTable(value);
  });

  // Hàm thực hiện tìm kiếm
  function searchTable(keyword) {
    const value = keyword.toLowerCase();

    $('.congVanTable tr').each(function () {
      const soHieu = $(this).find('.soHieu').text().toLowerCase();
      const ngayDi = $(this).find('.ngayDi').text().toLowerCase();
      const idNoiNhan = $(this).find('.idNoiNhan').text().toLowerCase();
      const idLoaiCongVan = $(this).find('.idLoaiCongVan').text().toLowerCase();
      const viTri = $(this).find('.viTri').text().toLowerCase();
      const noiDung = $(this).find('.noiDung').text().toLowerCase();
      const tep = $(this).find('.Tep').text().toLowerCase();
      const noiDungTep = $(this).find('.noiDungTep').text().toLowerCase();

      const match =
        soHieu.includes(value) ||
        ngayDi.includes(value) ||
        idNoiNhan.includes(value) ||
        idLoaiCongVan.includes(value) ||
        viTri.includes(value) ||
        noiDung.includes(value) ||
        tep.includes(value) ||
        noiDungTep.includes(value);

      $(this).toggle(match);
    });
  }

      