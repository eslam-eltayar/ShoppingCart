let dtable;
let currentFilter = 'all'; // Will be set based on page type
let filterFunction = null;

$(document).ready(function () {
    // Set default filter based on page type
    if (window.isTodayOrders === true) {
        currentFilter = 'pending';
        // Set Pending tab as active
        $('.btn-filter[data-filter="pending"]').addClass('active');
        $('.btn-filter[data-filter="all"]').removeClass('active');
    }
    
    loaddata();
    setupFilters();
});

function loaddata() {
    // Check if filterToday parameter exists in URL
    const urlParams = new URLSearchParams(window.location.search);
    const filterToday = urlParams.get('filterToday') === 'true';
    
    // Build URL with parameter
    let ajaxUrl = "/Admin/Order/GetData";
    if (filterToday) {
        ajaxUrl += "?filterToday=true";
    }
    
    // Remove previous filter if exists
    if (filterFunction) {
        $.fn.dataTable.ext.search.pop();
        filterFunction = null;
    }
    
    dtable = $("#myTable").DataTable({
        "ajax": {
            "url": ajaxUrl,
            "type": "GET",
            "dataSrc": "data"
        },
        "order": [[0, "desc"]], // Default sorting: Order ID descending (newest first)
        "columns": [
            { 
                "data": "id",
                "render": function (data) {
                    return `<span class="order-id">#${data}</span>`;
                }
            },
            { "data": "fullName" },
            { 
                "data": "phoneNumber",
                "render": function (data) {
                    if (data) {
                        return `<a href="tel:${data}" class="phone-link">${data}</a>`;
                    }
                    return "";
                }
            },
            { 
                "data": "orderStatus",
                "render": function (data, type, row) {
                    if (type === 'type' || type === 'filter') {
                        return data || '';
                    }
                    return getStatusBadge(data);
                }
            },
            { 
                "data": "totalPrice",
                "render": function (data) {
                    const price = parseFloat(data);
                    if (price === 0 || isNaN(price)) {
                        return '<span class="total-price zero-price">Price to confirm</span>';
                    }
                    return `<span class="total-price">${price.toFixed(2)} EGP</span>`;
                }
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<a href="/Admin/Order/Details?orderid=${data}" class="btn btn-sm btn-open-order">Open</a>`;
                },
                "orderable": false
            }
        ],
        "createdRow": function (row, data, dataIndex) {
            // Add class for pending orders
            const status = (data.orderStatus || '').toLowerCase();
            if (status === 'new' || status === 'pending') {
                $(row).addClass('order-pending');
            }
        },
        "drawCallback": function(settings) {
            // Update pending count
            updatePendingCount();
            
            // Show/hide empty state for Today's Orders only
            if (window.isTodayOrders === true) {
                const api = this.api();
                const visibleRows = api.rows({ filter: 'applied' }).count();
                if (visibleRows === 0) {
                    $('#myTable_wrapper').hide();
                    $('#empty-state').show();
                } else {
                    $('#myTable_wrapper').show();
                    $('#empty-state').hide();
                }
            }
        }
    });
    
    // Apply initial filter
    applyFilter();
}

function updatePendingCount() {
    if (!dtable) return;
    
    // Count pending orders
    let pendingCount = 0;
    dtable.rows().every(function() {
        const data = this.data();
        const status = (data.orderStatus || '').toLowerCase();
        if (status === 'new' || status === 'pending') {
            pendingCount++;
        }
    });
    
    // Update count display
    const countElement = $('#pending-count');
    if (pendingCount > 0) {
        countElement.text(` (${pendingCount})`);
    } else {
        countElement.text('');
    }
}

function getStatusBadge(status) {
    if (!status) {
        status = 'Pending';
    }
    
    const statusLower = status.toLowerCase();
    let badgeClass = 'pending';
    let displayText = status;
    
    if (statusLower === 'new' || statusLower === 'pending') {
        badgeClass = 'pending';
        displayText = 'Pending';
    } else if (statusLower === 'processing') {
        badgeClass = 'processing';
        displayText = 'Processing';
    } else if (statusLower === 'completed' || statusLower === 'approved') {
        badgeClass = 'completed';
        displayText = 'Approved';
    } else if (statusLower === 'cancelled') {
        badgeClass = 'cancelled';
        displayText = 'Cancelled';
    }
    
    return `<span class="order-status-badge ${badgeClass}">${displayText}</span>`;
}

function setupFilters() {
    $('.btn-filter').on('click', function () {
        // Update active button
        $('.btn-filter').removeClass('active');
        $(this).addClass('active');
        
        currentFilter = $(this).data('filter');
        applyFilter();
    });
}

function applyFilter() {
    if (!dtable) return;
    
    // Remove previous filter
    if (filterFunction) {
        $.fn.dataTable.ext.search.pop();
        filterFunction = null;
    }
    
    // Create new filter function
    // Note: 'data' parameter is an array of column values, not the object
    // We need to get the actual row data object using the DataTable API
    filterFunction = function(settings, data, dataIndex) {
        if (currentFilter === 'all') {
            return true;
        }
        
        // Get the actual row data object from DataTable
        const rowData = dtable.row(dataIndex).data();
        if (!rowData) return true;
        
        // Access orderStatus from the row data object
        const status = (rowData.orderStatus || '').toLowerCase();
        
        if (currentFilter === 'pending') {
            return status === 'new' || status === 'pending';
        } else if (currentFilter === 'approved') {
            return status === 'completed' || status === 'approved';
        }
        
        return true;
    };
    
    // Add filter and redraw
    $.fn.dataTable.ext.search.push(filterFunction);
    dtable.draw();
    
    // Update pending count after filter
    setTimeout(updatePendingCount, 100);
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
    
    


