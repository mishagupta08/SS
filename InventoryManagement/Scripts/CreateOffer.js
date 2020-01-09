var FromDate = "", ToDate = "", IsDateFilter=false;
var BuyProductArray = [], FreeProductArray = [];
var grid = null, FreeProductgrid = null;



$(document).ready(function () {
    $(".preloader").hide();
    
    var action = $("#ActionName").val();

    if (action.toLowerCase() == "edit") {
        getBuyProductList();
        getfreeProductList();        
    }
    else {
        $("#btnSave").attr('disabled', 'disabled');
        fillBuyProductGrid();
        fillFreeProductGrid();
    }

    getAllProductNames();

    $("#ProductName").autocomplete({
        source: function (request, response) {
            var results = $.ui.autocomplete.filter(ItemList, request.term);
            response(results.slice(0, 50));
        },
        minLength: 1,
        scroll: true,
        select: function (event, ui) {
            $("#ProductName").val(ui.item.label);
            SetSpecificCode(ui.item.label,"ProductCode");
            return false;
        },
        appendTo: $("#AddBuyProductdialog"),
    }).focus(function () {
        $(this).autocomplete("search", "");
    });

    $("#FreeProductName").autocomplete({
        source: function (request, response) {
            var results = $.ui.autocomplete.filter(ItemList, request.term);
            response(results.slice(0, 50));
        },
        minLength: 1,
        scroll: true,
        select: function (event, ui) {
            $("#FreeProductName").val(ui.item.label);
            SetSpecificCode(ui.item.label, "FreeProductCode");
            return false;
        },
        appendTo: $("#AddFreeProductdialog"),
    }).focus(function () {
        $(this).autocomplete("search", "");
    });
    $("#StartDate").datetimepicker({
        format: 'DD-MM-YYYY',
        widgetPositioning: {
            horizontal: 'auto',
            vertical: 'bottom'
        }

    }).on('dp.change', function (e) {
        var selectedDate = e.date._d;
        var lengthOfMonth = ((selectedDate.getMonth() + 1).toString()).length;
        var twoDigitMonth = (lengthOfMonth > 1) ? (selectedDate.getMonth() + 1) : '0' + (selectedDate.getMonth() + 1);
        var lengthOfDate = ((selectedDate.getDate()).toString()).length;
        var twoDigitdate = (lengthOfDate > 1) ? (selectedDate.getDate()) : '0' + (selectedDate.getDate());

        var newFromDate = twoDigitdate + "-" + twoDigitMonth + "-" + selectedDate.getFullYear();

        FromDate = newFromDate;
       
    });

    $("#EndDate").datetimepicker({
        format: 'DD-MM-YYYY',
        widgetPositioning: {
            horizontal: 'auto',
            vertical: 'bottom'
        }

    }).on('dp.change', function (e) {
        var selectedDate = e.date._d;
        var lengthOfMonth = ((selectedDate.getMonth() + 1).toString()).length;
        var twoDigitMonth = (lengthOfMonth > 1) ? (selectedDate.getMonth() + 1) : '0' + (selectedDate.getMonth() + 1);

        var lengthOfDate = ((selectedDate.getDate()).toString()).length;
        var twoDigitdate = (lengthOfDate > 1) ? (selectedDate.getDate()) : '0' + (selectedDate.getDate());

        var newFromDate = twoDigitdate + "-" + twoDigitMonth + "-" + selectedDate.getFullYear();

        ToDate = newFromDate;       
    });

    
    $("#btnReset").click(function () {
        resetform();
    });

    

    $("form[name=CreateOfferForm]").unbind("submit");
    $("form[name=CreateOfferForm]").bind('submit', function (e) {
        
        $(".preloader").show();
        var tempFreeArray = [];
        var validate = true;
        
        FromDate = $("#StartDate").val();
        ToDate = $("#EndDate").val();
        if (FromDate != "" && FromDate != null && ToDate != "" && ToDate != null) {
            var d1 = toDate(FromDate);
            var d2 = toDate(ToDate);
            if (d1 > d2) {
                validate = false;
                $(".preloader").hide();
                OpenDialog("dialogMessage", "From Date should be less than To Date!", "false");
            }
        }
        else {
            validate = false;
            $(".preloader").hide();
            OpenDialog("dialogMessage", "Please add offer start and end date.", "false");
        }

        if (validate) {
            if (FreeProductArray.length <= 0) 
            {
                OpenDialog("dialogMessage", "Please add a free product.", "false");
                validate = false;
                $(".preloader").hide();               
            }
        }

        if (validate == true)
        {
            var ProductArraystring = JSON.stringify(BuyProductArray);
            $("#ParentProductString").val(ProductArraystring);

            var FreeProductArraystring = JSON.stringify(FreeProductArray);
            $("#ProductString").val(FreeProductArraystring);

            var formdata = new FormData($(this)[0]);

            $.ajax({
                url: '/Transaction/SaveBuyThisGetThatOffer',
                type: 'POST',
                data: formdata,
                processData: false,
                contentType: false,
                dataType: "json",
                success: function (objResponse) {
                    $(".preloader").hide();
                    if (objResponse != null) {
                        if (objResponse != null) {
                            if (objResponse.ResponseStatus == "OK") {
                                OpenDialog("dialogMessage", "Saved Successfully", "false");
                                window.location.href = "BuyThisGetThatOfferMaster";
                            }
                            else {
                                OpenDialog("dialogMessage", objResponse.ResponseMessage, "false");
                            }
                        }
                    }
                },
                error: function (xhr, data) {
                    $(".preloader").hide();
                    console.log(xhr);
                    console.log("Error:", data);
                }
            });

        }
        return false;
    });

    $("#AddBuyProduct").click(function () {
        $("#AddBuyProductdialog").dialog({
            modal: true,
            width: "60%"
        });
    });

    $("#AddFreeProduct").click(function () {
        if (BuyProductArray.length > 0) {
            $("#AddFreeProductdialog").dialog({
                modal: true,
                width: "60%"
            });
        }
        else {
            OpenDialog("dialogMessage", "Please add buy products first.", "false");
        }
    });

    $("#YesBtn").click(function () {
        var objIndex = 0;
        var code = $("#ProductCode").val();
        var Name = $("#ProductName").val();
        var BuyQuantity = $("#BuyQuantity").val();

        var product = { "ProdCode": code, "ProductName": Name, "BuyQuantity": BuyQuantity, "BuyProduct": true };

        if (BuyProductArray.length > 0) {
            objIndex = BuyProductArray.findIndex((obj => obj.ProdCode == code));
            if (objIndex === -1) {
                BuyProductArray.push(product);
            }
            else {
                BuyProductArray[objIndex] = product;
            }
        }
        else {
            BuyProductArray.push(product);
        }

        fillBuyProductGrid();
        ClearBuyProductdetails();
        $("#AddBuyProductdialog").dialog('close');
        $("#btnSave").prop('disabled', false);
    });


    $("#FreeYesBtn").click(function () {
        var objIndex = 0;
        var code = $("#FreeProductCode").val();
        var Name = $("#FreeProductName").val();
        var FreeQuantity = $("#FreeProdQuantity").val();
        var BVApplied = $("#BVApplied").val();

        var product = { "ProdCode": code, "ProductName": Name, "FreeQuantity": FreeQuantity,"BVApplied":BVApplied, "BuyProduct": false };

        if (FreeProductArray.length > 0) {
            objIndex = FreeProductArray.findIndex((obj => obj.ProdCode == code));
            if (objIndex === -1) {
                FreeProductArray.push(product);
            }
            else {
                FreeProductArray[objIndex] = product;
            }
        }
        else {
            FreeProductArray.push(product);
        }

        fillFreeProductGrid();
        ClearFreeProductdetails();
        $("#AddFreeProductdialog").dialog('close');
        $("#btnSave").prop('disabled', false);
    });

});

function resetform() {
    $('#CreateOfferForm')[0].reset();
    BuyProductArray = [{}];
    FreeProductArray = [{}];
    fillBuyProductGrid();
    fillFreeProductGrid();
}

function toDate(dateStr) {
    var parts = dateStr.split("-");
    return new Date(parts[2], parts[1] - 1, parts[0]);
}

function OpenDialog(dialogId, Message, isConfirmation) {
    $("#" + dialogId).empty();
    $("#" + dialogId).append('<p>' + Message + '</p>');
    if (isConfirmation == "true") {
        $("#" + dialogId).dialog({
            modal: true,
            buttons: [{
                text: "Yes",
                id: "btnYes" + dialogId,
                click: function () {
                    IsYes = true;
                    $("#" + dialogId).dialog("close");
                }
            },
            {
                text: "No",
                id: "btnNo" + dialogId,
                click: function () {
                    IsYes = false;
                    $("#" + dialogId).dialog("close");
                }
            }
            ]
        });
        $(".ui-dialog-titlebar-close").empty();
        $(".ui-dialog-titlebar-close").append('<i class="fa fa-close"></i>');
    }
    else {


        $("#" + dialogId).dialog({
            modal: true,
            buttons: [{
                text: "OK",
                id: "btn" + dialogId,
                click: function () {
                    $("#" + dialogId).dialog("close");
                }
            }]
        });
        $(".ui-dialog-titlebar-close").empty();
        $(".ui-dialog-titlebar-close").append('<i class="fa fa-close"></i>');
    }

}

function SetSpecificCode(label,id) {
    var objIndex = FullProductList.findIndex((obj => obj.ProductName == label));
    if (objIndex != -1) {
        $("#"+id).val(parseFloat(FullProductList[objIndex].ProductCode));
    }
}

function ClearBuyProductdetails() {
    $("#ProductName").val("");
    $("#ProductCode").val("");
    $("#BuyQuantity").val("0");    
}

function ClearFreeProductdetails() {
    $("#ProductName").val("");
    $("#ProductCode").val("");   
    $("#FreeQty").val("");
    $("#BVApplied").val("Y");
    
}

function getAllProductNames() {
    $.ajax({
        url: '/Transaction/GetProductList',
        dataType: 'JSON',
        method: 'GET',
        success: function (data) {
            ItemList = [];
            if (data != null) {
                FullProductList = data;
                ItemList = data.map(a => a.ProductName);
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function fillProductValues(objResult) {

    if (ProductArray[0].ProdCode == "" || ProductArray[0].ProdCode == null || ProductArray[0].ProdCode == undefined) {
        ProductArray.splice(0, 1);
    }

    ProductArray.push({ "AvailStock": objResult.StockAvailable, "ProductName": objResult.ProductName, "Barcode": objResult.Barcode, "BatchNo": objResult.BatchNo, "ProdCode": objResult.ProductCodeStr, "MRP": objResult.MRP, "DP": objResult.DP, "RP": objResult.RP, "BV": objResult.BV, "CV": objResult.CV, "PV": objResult.PV, "CommsnPer": objResult.CommissionPer, "DiscPer": objResult.DiscPer, "TaxPer": objResult.TaxPer, "Rate": 0, "Quantity": 0, "FreeQuantity": 0 });    
    $("#ProductName").val(objResult.ProductName);
    $("#Batch").val(objResult.BatchNo);
    $("#MRP").val(objResult.MRP);
    $("#DP").val(objResult.DP);
    $("#BV").val(objResult.BV);
    $("#FreeQty").val(0);     
}

function DeleteBuyProduct(ev) {
    BuyProductArray = $.grep(BuyProductArray, function (e) {
        return e.ProdCode != ev.data.record.ProdCode;
    });
    fillBuyProductGrid();
}

function fillBuyProductGrid() {
    $("#noRecord").hide();
    $(".preloader").hide();

    if (grid != null) {
        grid.destroy();
        $('#grid').empty();
    }

    grid = $('#grid').grid({
        dataSource: BuyProductArray,
        uiLibrary: 'bootstrap',
        headerFilter: false,
        columns: [
            { field: 'ProdCode', title: 'Product Code', width: 100, sortable: true, hidden: false, filterable: false },
            { field: 'ProductName', title: 'Product Name', width: 100, sortable: true, hidden: false, filterable: true },                        
            { field: 'BuyQuantity', title: 'Buy Qty', width: 120, sortable: true, hidden: false, filterable: true },
            { field: 'BuyProduct', title: 'Buy Product', width: 120, sortable: true, hidden: true, filterable: true },
            { title: '', field: 'Delete', width: 34, type: 'icon', width: 70, icon: 'glyphicon-remove', tooltip: 'Edit', events: { 'click': DeleteBuyProduct }, filterable: false }
            
        ],
        pager: { limit: 20, sizes: [15, 20, 35, 50, 65, 80, 95, 100] }
    });       
}

function DeleteFreeProduct(ev) {
    FreeProductArray = $.grep(FreeProductArray, function (e) {
        return e.ProdCode != ev.data.record.ProdCode;
    });
    fillFreeProductGrid();
}

function fillFreeProductGrid() {

    $("#noRecord").hide();
    $(".preloader").hide();
    if (FreeProductgrid != null) {
        FreeProductgrid.destroy();
        $('#FreeProductgrid').empty();
    }

    FreeProductgrid = $('#FreeProductgrid').grid({
        dataSource: FreeProductArray,
        uiLibrary: 'bootstrap',
        headerFilter: false,
        columns: [
            { field: 'ProdCode', title: 'Enter Product Code',  width: 100, sortable: true, hidden: false, filterable: false },
            { field: 'ProductName', title: 'Product Name', width: 100, sortable: true, hidden: false, filterable: true },                        
            { field: 'BVApplied', title: 'BV Applied', width: 100, sortable: true, hidden: false, filterable: true },
            { field: 'FreeQuantity', title: 'Free Qty', width: 120, sortable: true, hidden: false, filterable: true },
            { title: '', field: 'Delete', width: 34, type: 'icon', width: 70, icon: 'glyphicon-remove', tooltip: 'Edit', events: { 'click': DeleteFreeProduct }, filterable: false }
            
        ],
        pager: { limit: 20, sizes: [15, 20, 35, 50, 65, 80, 95, 100] }
    });
    
}

function getfreeProductList() {
    var AID = $("#AID").val();
    $(".preloader").show();
    $.ajax({
        url: '/Transaction/getExtraPVfreeproducts',
        dataType: 'JSON',
        method: 'GET',
        data: { "id": AID },
        success: function (data) {
            FreeProductArray = [];
            for (var i = 0; i < data.length; i++) {
                FreeProductArray.push({ "ProdCode": data[i].ProductCode, "ProductName": data[i].ProductName, "FreeQuantity": data[i].Qty, "BuyProduct": false, "BVApplied": data[i].IsBvApplied });
            }
            fillFreeProductGrid();
        },
        error: function (error) {
            console.log(error);
        }
    });
}


function getBuyProductList() {
    var AID = $("#AID").val();
    $(".preloader").show();
    $.ajax({
        url: '/Transaction/getOfferBuyproducts',
        dataType: 'JSON',
        method: 'GET',
        data: { "id": AID },
        success: function (data) {
            BuyProductArray = [];
            for (var i = 0; i < data.length; i++) {
               BuyProductArray.push({ "ProdCode": data[i].ProductCode, "ProductName": data[i].ProductName, "BuyQuantity": data[i].Qty,"BuyProduct": true });
            }
            fillBuyProductGrid();
        },
        error: function (error) {
            console.log(error);
        }
    });
}