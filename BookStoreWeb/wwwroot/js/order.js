
let dataTable;

$(document).ready(function () {

    var url = window.location.search;
    if (url.includes("inprocess")) {



        loadDataTable("inprocess");
    }
    else if (url.includes("pending")) {

        loadDataTable("pending");
    }
    else if (url.includes("completed")) {

        loadDataTable("completed");
    }

    else if (url.includes("approved")) {

        loadDataTable("approved");
    }

    else {
        loadDataTable("all");
    }


});

function loadDataTable(status) {
    dataTable = $('#tbldata').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll?status="+status
        },

        "columns": [
            { "data": "id", "width": "15%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {

                    return `
                     <a href="/Admin/Order/Details?OrderId=${data}"  class="btn btn-primary">Details</a>
                      
                    `
                },


                "width": "15%"
            },
        ]

    });

}

