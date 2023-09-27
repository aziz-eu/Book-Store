
let dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tbldata').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },

        "columns": [
            { "data": "title", "width": "15%" },
            { "data": "isbn", "width": "15%" },
            { "data": "author", "width": "15%" },
            { "data": "catagory.name", "width": "15%" },
            { "data": "price", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {

                    return `
                     <a href="/Admin/Product/Upsert?id=${data}"  class="btn btn-primary">Edit</a>
                        <a  class="btn btn-danger">Delete</a>
                    `
                },


                "width": "15%"
            },
        ]

    });

}