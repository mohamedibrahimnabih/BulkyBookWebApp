$(document).ready(function () {
    $('#tableData').DataTable({
        ajax: {
            url: '/Admin/Product/GetAll',
            dataSrc: ''
        },
        columns: [
            { data: 'title', defaultContent: 'N/A' },
            { data: 'isbn', defaultContent: 'N/A' },
            { data: 'author', defaultContent: 'N/A' },
            { data: 'price', defaultContent: 'N/A' },
            { data: 'category.name', defaultContent: 'N/A' },
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
        var productId = data.id;

        // Redirect to the edit page
        window.location.href = '/Admin/Product/UpSert/' + productId;
    });

    // Handle delete button click
    $('#tableData').on('click', '.delete-button', function () {
        var data = $('#tableData').DataTable().row($(this).parents('tr')).data();
        var productId = data.id;
        var productName = data.title;

        var deleteForm = document.getElementById('deleteForm');
        deleteForm.action = '/Admin/Product/Delete/' + productId;

        var productNameSpan = document.getElementById('productName');
        productNameSpan.textContent = productName;
    });
});