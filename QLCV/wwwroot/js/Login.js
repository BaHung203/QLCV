$(document).ready(function (){
     $('.btn-delete').click(function () {
        if (confirm('Bạn có chắc chắn muốn xóa phòng ban này?')) {
            const id = $(this).data('id');
            $.post('/Login/Delete', { id: id }, function () {
                location.reload();
            });
        }
    });
})