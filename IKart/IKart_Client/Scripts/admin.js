//// Basic admin JS helpers
//$(function () {
//    $('#sidebarToggle').click(function (e) {
//        e.preventDefault();
//        $('#admin-sidebar').toggleClass('show');
//    });



//    // Generic modal confirm (used by delete buttons etc.)
//    window.confirmAction = function (message, callback) {
//        if (confirm(message)) callback();
//    };


//    // DataTables auto-init for tables with id ending with Table
//    $('table[id$="Table"]').each(function () {
//        if (!$.fn.DataTable.isDataTable(this)) {
//            $(this).DataTable();
//        }
//    });
//});

// Basic admin JS helpers
$(function () {
    // Sidebar toggle with smooth animation
    $('#sidebarToggle').on('click', function (e) {
        e.preventDefault();
        $('#admin-sidebar').toggleClass('show');
    });

    // Generic modal confirm (used by delete buttons etc.)
    window.confirmAction = function (message, callback) {
        const modalMessage = message || "Are you sure you want to proceed?";
        if (confirm(modalMessage)) {
            if (typeof callback === 'function') {
                callback();
            }
        }
    };

    // DataTables auto-init for tables with id ending in 'Table'
    $('table[id$="Table"]').each(function () {
        if (!$.fn.DataTable.isDataTable(this)) {
            $(this).DataTable({
                responsive: true,
                pageLength: 10,
                language: {
                    searchPlaceholder: "Search records...",
                    lengthMenu: "Show _MENU_ entries",
                    zeroRecords: "No matching records found",
                    info: "Showing _START_ to _END_ of _TOTAL_ entries",
                    paginate: {
                        previous: "<",
                        next: ">"
                    }
                }
            });
        }
    });
});
