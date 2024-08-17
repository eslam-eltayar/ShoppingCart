$(document).ready(function () {
    loaddata();
});

function loaddata() {
    dtable = $("#myTable").DataTable({
        "ajax": {
            "url": "/Admin/Order/GetData",
            "dataSrc": "data"
        },
        "columns": [
            { "data": "id" },
            { "data": "name" },
            { "data": "phone" },
            { "data": "appUser.email" },
            { "data": "orderStatus" },
            { "data": "totalPrice" },
            {

                "data": "id",
                "render": function (data) {
                    return `
                    <a href="/Admin/Order/Details?orderid=${data}" class="btn btn-warning"> Details </a>
                    `
                }
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
    
    


