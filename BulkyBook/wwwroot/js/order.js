$(document).ready(function () {
    $('#tableData').DataTable({
        ajax: {
            url: '/Admin/Order/GetAll',
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

    // Handle edit button click
    $('#tableData').on('click', '.view-button', function (e) {
        e.preventDefault();
        var data = $('#tableData').DataTable().row($(this).parents('tr')).data();
        var orderId = data.id;

        // Redirect to the edit page
        window.location.href = '/Admin/Order/Details/' + orderId;
    });
});