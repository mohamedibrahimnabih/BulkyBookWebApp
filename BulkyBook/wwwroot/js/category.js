$(document).ready(function () {
    $('#tableData').DataTable({
        ajax: {
            url: '/Admin/Category/GetAll',
            dataSrc: ''
        },
        columns: [
            { data: 'name', defaultContent: 'N/A' },
            { data: 'displayOrder', defaultContent: 'N/A' },
            {
                data: null,
                className: 'dt-center',
                defaultContent: `
                                    <a href="" class="btn btn-warning btn-sm edit-button"><i class="bi bi-pencil-square"></i> Edit</a>
                                    <button class="btn btn-danger btn-sm delete-button" data-bs-toggle="modal" data-bs-target="#deleteModal"><i class="bi bi-trash"></i> Delete</button>
                                `,
                orderable: false
            }
        ]
    });

    // Handle edit button click
    $('#tableData').on('click', '.edit-button', function (e) {
        e.preventDefault();
        var data = $('#tableData').DataTable().row($(this).parents('tr')).data();
        var categoryId = data.id;

        // Redirect to the edit page
        window.location.href = '/Admin/Category/UpSert/' + categoryId;
    });

    // Handle delete button click
    $('#tableData').on('click', '.delete-button', function () {
        var data = $('#tableData').DataTable().row($(this).parents('tr')).data();
        var categoryId = data.id;
        var categoryName = data.name;

        var deleteForm = document.getElementById('deleteForm');
        deleteForm.action = '/Admin/Category/Delete/' + categoryId;

        var categoryNameSpan = document.getElementById('categoryName');
        categoryNameSpan.textContent = categoryName;
    });
});