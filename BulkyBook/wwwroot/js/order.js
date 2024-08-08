$(document).ready(function () {
    var table = $('#tableData').DataTable({
        ajax: {
            url: '/Admin/Order/GetAll',
            data: function (d) {
                d.status = selectedStatus;  // Send selected status to server
            },
            dataSrc: ''
        },
        columns: [
            { data: 'id', defaultContent: 'N/A' },
            { data: 'name', defaultContent: 'N/A' },
            { data: 'phoneNumber', defaultContent: 'N/A' },
            { data: 'applicationUser.email', defaultContent: 'N/A' },
            { data: 'orderStatus', defaultContent: 'N/A' },
            { data: 'orderTotal', defaultContent: 'N/A' },
            {
                data: null,
                className: 'dt-center',
                defaultContent: `
                    <a href="" class="btn btn-warning btn-sm view-button"><i class="bi bi-box-arrow-up-right"></i> View</a>
                `,
                orderable: false
            }
        ]
    });

    var selectedStatus = "All";  // Default status is "All"

    // Handle filter click
    $('.list-group-item').click(function () {
        selectedStatus = $(this).parent().data('status');

        $('.list-group-item').removeClass('active text-white bg-dark');
        $(this).addClass('active text-white bg-dark');

        table.ajax.reload();
    });

    // Handle view button click
    $('#tableData').on('click', '.view-button', function (e) {
        e.preventDefault();
        var data = table.row($(this).parents('tr')).data();
        var orderId = data.id;
        window.location.href = '/Admin/Order/Details/' + orderId;
    });
});
