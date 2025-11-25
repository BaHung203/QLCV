document.addEventListener('DOMContentLoaded', function () {
    const filter = document.getElementById('loaiFilter');
    const table = document.getElementById('congVanTable').getElementsByTagName('tbody')[0];

    filter.addEventListener('change', function () {
        const value = this.value; // CongVanDen, CongVanDi hoặc ""
        const rows = table.getElementsByTagName('tr');

        for (let i = 0; i < rows.length; i++) {
            const loai = rows[i].cells[3].innerText; // cột Loại
            if (value === "" || loai === value) {
                rows[i].style.display = "";
            } else {
                rows[i].style.display = "none";
            }
        }
    });
});
