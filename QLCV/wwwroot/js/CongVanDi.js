document.addEventListener("DOMContentLoaded", function () {
    
  });
  function loadEditNoiNhan() {
    $.ajax({
        url: '/congVanDi/GetNoiNhan', // đổi đúng tên controller/action bạn tạo
        type: 'GET',
        success: function (data) {
            const select = $('#editIdNoiNhan');
            select.empty(); // xóa option cũ
            select.append('<option value="">-- Chọn nơi nhận --</option>');

            $.each(data, function (i, item) {
                select.append(`<option value="${item.id}">${item.tenNoiNhan}</option>`);
            });
        },
        error: function () {
            console.log("Lỗi khi tải danh sách nơi nhận");
        }
    });
}
$(document).ready(function() {
    $('.btnEdit').on('click', function () {
        const btn = $(this);
        var id = btn.data('id');
        var data = btn.data('item');
        const noiNhanId = $(this).data('noinhan');

        loadEditNoiNhan();
         if (typeof data === 'string') {
            data = JSON.parse(data);
        }
        // Hiển thị modal trước
        $('#modalEditCongVan').modal("show");

        // Đợi modal hiển thị xong rồi set giá trị
        $('#modalEditCongVan').one('shown.bs.modal', function () {
          $('#modalEditCongVan input[name="Id"]').val(data.ID);
          $('#modalEditCongVan input[name="SoHieu"]').val(data.SoHieu);
          $('#modalEditCongVan input[name="Ngay"]').val(formatDate(data.Ngay));
          $('#modalEditCongVan select[name="IdNoiNhan"]').val(data.IdNoiNhan);
          $('#modalEditCongVan input[name="ViTri"]').val(data.ViTri);
          $('#modalEditCongVan textarea[name="NoiDung"]').val(data.NoiDung);
          $('#fileNameDisplay').text(data.TepDinhKem ? `Đã đính kèm: ${data.TepDinhKem}` : '');
          $('#modalEditCongVan textarea[name="NoiDungTep"]').val(data.NoiDungTep);

          const fileNameText = data.TepDinhKem ? `Đã đính kèm: ${data.TepDinhKem}` : '';
          $('#fileNameDisplay').text(fileNameText);
          loadEditNoiNhan(data.id);
           $('#modalEditCongVan').modal("show");
            setTimeout(() => $('#editIdNoiNhan').val(noiNhanId), 300);
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
   

$('.btn-Details').on('click', function () {
    const id = $(this).data('id');
    window.location.href = `/CongVanDi/Details/${id}`;
});

$('#searchInput').on('keyup', function () {
    const value = $(this).val();
    searchTable(value);
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
      const Ngay = $(this).find('.Ngay').text().toLowerCase();
      const idNoiNhan = $(this).find('.IdNoiNhan').text().toLowerCase();
      const viTri = $(this).find('.viTri').text().toLowerCase();
      const noiDung = $(this).find('.noiDung').text().toLowerCase();
      const tep = $(this).find('.Tep').text().toLowerCase();
      const noiDungTep = $(this).find('.noiDungTep').text().toLowerCase();

      const match =
        soHieu.includes(value) ||
        Ngay.includes(value) ||
        idNoiNhan.includes(value) ||
        viTri.includes(value) ||
        noiDung.includes(value) ||
        tep.includes(value) ||
        noiDungTep.includes(value);

      $(this).toggle(match);
    });
  }
});
      