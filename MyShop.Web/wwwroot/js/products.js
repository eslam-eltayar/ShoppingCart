$(document).ready(function () {
    loaddata();
});

function loaddata() {
    dtable = $("#myTable").DataTable({
        "ajax": {
            "url": "/Admin/Product/GetData",
            "dataSrc": "data"
        },
        "order": [[0, "desc"]], // Default sorting: Name descending
        "columns": [
            { "data": "name" },
            { "data": "description" },
            { 
                "data": "price",
                "render": function (data) {
                    const price = parseFloat(data);
                    return `<span class="price-value">${price.toFixed(2)} EGP</span>`;
                }
            },
            { "data": "category.name" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a href="/Admin/Product/Edit/${data}" class="btn btn-sm btn-action btn-edit">Edit</a>
                    <a onClick="DeleteItem('/Admin/Product/Delete/${data}')" class="btn btn-sm btn-action btn-delete">Delete</a>
                    `
                },
                "orderable": false
            }
        ]
    });
}

function DeleteItem(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "Delete",
                success: function (data) {
                    if (data.success) {
                        dtable.ajax.reload();
                        toaster.success(data.message);
                    }
                    else {
                        toaster.error(data.message);
                    }
                }
            });

            Swal.fire(
                'Deleted!',
                'Your file has been deleted.',
                'success'

            )
        }
    })
}
    
    


