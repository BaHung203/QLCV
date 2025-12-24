document.addEventListener('DOMContentLoaded', function () {

    const badge = document.getElementById('notificationBadge');
    const list = document.getElementById('notificationList');

    // Kết nối tới Hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .build();

    connection.on("ReceiveNotification", (title, message) => {
        const li = document.createElement("li");
        li.innerHTML = `
            <a class="dropdown-item" href="#">
                <strong>${title}</strong><br>
                <small class="text-muted">${message}</small>
            </a>`;
        list.prepend(li);

        let count = parseInt(badge.textContent) || 0;
        badge.textContent = count + 1;
    });


    connection.start().catch(err => console.error(err.toString())); // Tự động reload mỗi 10s


    // ================= Chart.js =================
    var ctx = document.getElementById('documentChart');
    if (ctx) {
        var myChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6'],
                datasets: [
                    {
                        label: 'Công văn đến',
                        data: [12, 19, 10, 15, 22, 30],
                        backgroundColor: 'rgba(59, 130, 246, 0.1)',
                        borderColor: 'rgba(59, 130, 246, 1)',
                        borderWidth: 2,
                        pointBackgroundColor: 'rgba(59, 130, 246, 1)',
                        tension: 0.3
                    },
                    {
                        label: 'Công văn đi',
                        data: [8, 15, 7, 12, 17, 25],
                        backgroundColor: 'rgba(16, 185, 129, 0.1)',
                        borderColor: 'rgba(16, 185, 129, 1)',
                        borderWidth: 2,
                        pointBackgroundColor: 'rgba(16, 185, 129, 1)',
                        tension: 0.3
                    }
                ]
            },
            options: {
                maintainAspectRatio: false,
                scales: {
                    y: { beginAtZero: true }
                },
                plugins: {
                    legend: { position: 'top' }
                }
            }
        });
    }

});
