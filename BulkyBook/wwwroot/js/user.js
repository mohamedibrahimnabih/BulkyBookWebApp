$(document).ready(function () {
    $('#tableData').DataTable({
        ajax: {
            url: '/Admin/User/GetAll',
            dataSrc: ''
        },
        columns: [
            { data: 'name', defaultContent: 'N/A' },
            { data: 'email', defaultContent: 'N/A' },
            { data: 'phoneNumber', defaultContent: 'N/A' },
            { data: 'company.name', defaultContent: 'N/A' },
            { data: 'role', defaultContent: 'N/A' },
            {
                data: null,
                className: 'dt-center',
                render: function (data, type, row) {
                    if (row.lockoutEnd != null) {
                        return `<a href="javascript:;" class="btn btn-success btn-sm unlock-button" data-id="${row.id}"><i class="bi bi-unlock-fill"></i> Unlock</a>`;
                    } else {
                        return `<a href="javascript:;" class="btn btn-danger btn-sm lock-button" data-id="${row.id}"><i class="bi bi-lock-fill"></i> Lock</a>`;
                    }
                },
                orderable: false
            },
            {
                data: null,
                className: 'dt-center',
                render: function (data, type, row) {
                    return `<a href="/Admin/User/RoleManagement?userId=${row.id}" class="btn btn-warning btn-sm"><i class="bi bi-pencil-square"></i> Manage Role</a>`;
                },
                orderable: false
            }
        ]
    });

    // Handle lock/unlock button click
    $('#tableData').on('click', '.lock-button, .unlock-button', function (e) {
        e.preventDefault();
        const userId = $(this).data('id');

        $.ajax({
            type: 'POST',
            url: '/Admin/User/LockUnlock',
            contentType: 'application/json',
            data: JSON.stringify(userId),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    $('#tableData').DataTable().ajax.reload();
                } else {
                    toastr.error(response.message);
                }
            }
        });
    });
});
