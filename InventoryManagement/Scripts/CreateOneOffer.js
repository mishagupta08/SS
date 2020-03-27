var FromDate = "", ToDate = "", IdDate = "", IsDateFilter = false;
var FreeProductArray = [];
var BuyProductArray = [];
var FreeProductgrid = null;
var BuyProductgrid = null;
var ItemList = [], FullProductList = [];
var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

$(document).ready(function () {
    $(".preloader").hide();
    var PreviouslySelectedFromDate = "";
    var PreviouslySelectedToDate = "";
    var PreviouslySelectedFromPickerDate = "", type = "";
    var PreviouslySelectedToPickerDate = "", newToDate = "";
    var Action = $("#Action").val();

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
            SetSpecificCode(ui.item.label, "ProductCode");
            return false;
        },
        appendTo: $("#AddProductdialog"),
    }).focus(function () {
        $(this).autocomplete("search", "");
    });


    $("#BuyProductName").autocomplete({
        source: function (request, response) {
            var results = $.ui.autocomplete.filter(ItemList, request.term);
            response(results.slice(0, 50));
        },
        minLength: 1,
        scroll: true,
        select: function (event, ui) {
            $("#BuyProductName").val(ui.item.label);
            SetSpecificCode(ui.item.label, "BuyProductCode");
            return false;
        },
        appendTo: $("#AddBuyProductdialog"),
    }).focus(function () {
        $(this).autocomplete("search", "");
    });


    if (Action.toLowerCase() == "edit") {
        GetOfferProduct();
        GetOfferBuyProduct();
        var party = $("#Party").val();
        if (party == "all") {
            $('#OfferPartyList option').prop('selected', true);
            $('#Party').html("all");
        }
        else if (party != null && party != "" && party != undefined) {
            var dataarray = party.split(",");
            $("#OfferPartyList").val(dataarray);
        }
    }
    else {
        $("#btnSave").prop('disabled', true);
        fillFreeProductGrid();
    }

    //$("#OfferOnMinBV").focusout(function () {
    //    var minbv = $("#OfferOnMinBV").val();
    //    if (minbv != null && minbv != "" && minbv!=undefined)
    //    {
    //        var NewName = minbv + " FV";
    //        $("#OfferName").val(NewName);
    //    }

    //});
    var dateToday = new Date();

    $("#StartDate").datetimepicker({
        format: "DD-MMM-YYYY",
        extraFormats: ['M/D/YYYY', 'MM/DD/YYYY'],
        widgetPositioning: {
            horizontal: 'auto',
            vertical: 'bottom'
        }

    });

    $("#EndDate").datetimepicker({
        format: "DD-MMM-YYYY",
        extraFormats: ['M/D/YYYY', 'MM/DD/YYYY'],
        widgetPositioning: {
            horizontal: 'auto',
            vertical: 'bottom'
        }

    });

    $("#IdDate").datetimepicker({
        format: "DD-MMM-YYYY",
        extraFormats: ['M/D/YYYY', 'MM/DD/YYYY'],
        widgetPositioning: {
            horizontal: 'auto',
            vertical: 'bottom'
        }
    });


    $("#btnReset").click(function () {
        resetform();
    });

    $("form[name=CreateOtherOfferForm]").unbind("submit");
    $("form[name=CreateOtherOfferForm]").bind('submit', function (e) {
        var str = $("#StartDate").val().split("-");
        var fromd = new Date(str[2], months.indexOf(str[1]) + 1, str[0]);

        str = $("#EndDate").val().split("-");
        var newend = new Date(str[2], months.indexOf(str[1]) + 1, str[0]).addDays(1);
        var current = new Date();

        //if (fromd <= current && newend >= current) {
        $(".preloader").show();
        var formdata = new FormData($(this)[0]);
        $.ajax({
            url: '/Transaction/OtherOfferSave',
            type: 'POST',
            data: formdata,
            processData: false,
            contentType: false,
            dataType: "json",
            success: function (objResponse) {
                $(".preloader").hide();
                if (objResponse != null) {
                    if (objResponse.ResponseStatus == "OK") {
                        OpenDialog("dialogMessage", "Saved Successfully", "false");
                        window.location.href = "/Transaction/OtherOfferMaster";
                    }
                    else {
                        OpenDialog("dialogMessage", objResponse.ResponseMessage, "false");
                    }
                }
            },
            error: function (xhr, data) {
                $(".preloader").hide();
                console.log(xhr);
                console.log("Error:", data);
            }
        });
        //}
        //else {
        //    OpenDialog("dialogMessage", "Offer can't be edited.", "false");
        //}
        return false;
    });

    $("form[name=CreateOfferForm]").unbind("submit");
    $("form[name=CreateOfferForm]").bind('submit', function (e) {

        $(".preloader").show();
        var tempFreeArray = [];
        var validate = true;
        var type = $("#offerType").val();
        $.each(FreeProductArray, function (key, value) {

            if (value.ProdCode != "" && value.ProdCode != null && value.ProdCode != undefined) {


                if (value.Qty == "" || value.Qty == null || value.Qty == undefined) {
                    OpenDialog("dialogMessage", "Please Enter Qty for Free product : " + value.ProdCode, "false");
                    validate = false; $(".preloader").hide();
                    return false;
                }
                if (value.Flexible == "" || value.Flexible == null || value.Flexible == undefined) {
                    OpenDialog("dialogMessage", "Please select Is Flexible for Free product : " + value.ProdCode, "false");
                    validate = false; $(".preloader").hide();
                    return false;
                }

                if (validate == true) {
                    tempFreeArray.push(value);
                }
            }
        });

        if (validate) {
            if (tempFreeArray.length == 0) {
                OpenDialog("dialogMessage", "Please add a free product.", "false");
                validate = false;
                $(".preloader").hide();
            }
        }

        if (validate == true) {

            var FreeProductArraystring = JSON.stringify(tempFreeArray);
            $("#PrductString").val(FreeProductArraystring);

            var BuyProductArraystring = JSON.stringify(BuyProductArray);
            $("#BuyPrductString").val(BuyProductArraystring);

            var formdata = new FormData($(this)[0]);

            $.ajax({
                url: 'SaveOffer',
                type: 'POST',
                data: formdata,
                processData: false,
                contentType: false,
                dataType: "json",
                success: function (objResponse) {
                    $(".preloader").hide();
                    if (objResponse != null) {
                        if (objResponse.ResponseStatus == "OK") {
                            OpenDialog("dialogMessage", "Saved Successfully", "false");
                            if (type == "1")
                                window.location.href = "OfferMaster";
                            else if (type == "2")
                                window.location.href = "OneRupeeOfferMaster";
                            else if (type == "3")
                                window.location.href = "FreeProductOfferMaster";
                        }
                        else {
                            OpenDialog("dialogMessage", objResponse.ResponseMessage, "false");
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


    $("#YesBtn").click(function () {
        var objIndex = 0;
        var code = $("#ProductCode").val();
        var Qty = $("#Quantity").val();
        var Name = $("#ProductName").val();
        var FreeQty = $("#FreeQuantity").val();
        var IsFlexible = $("#IsFlexible").val();
        var OfferMrp = $("#OfferMRP").val();

        var product = { "ProdCode": code, "ProductName": Name, "Qty": Qty, "FreeQty": FreeQty, "Flexible": IsFlexible, "OfferMrp": OfferMrp };

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
        $("#AddProductdialog").dialog('close');
        $("#btnSave").prop('disabled', false);
    });

    $("#BuyYesBtn").click(function () {
        var objIndex = 0;
        var code = $("#BuyProductCode").val();
        var Qty = $("#BuyQuantity").val();
        var Name = $("#BuyProductName").val();
        var FreeQty = $("#BuyQuantity").val();
        var IsFlexible = $("#IsFlexible").val();
        var OfferMrp = $("#OfferMRP").val();

        var product = { "ProdCode": code, "ProductName": Name, "Qty": Qty };

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
        $("#AddBuyProductdialog").dialog('close');
    });

    $("#Quantity").focusout(function () {
        $("#FreeQuantity").val($("#Quantity").val());
    });

    $("#IsFlexible").change(function () {
        var oldValue = $("#IsFlexible").val();
        if (oldValue != null && oldValue != "" && oldValue != undefined && oldValue == "N") {
            $("#FreeQuantity").prop("readonly", true);
        }
        else {
            $("#FreeQuantity").prop("readonly", false);
        }
    });

    $('#select_all').click(function () {
        $('#OfferPartyList option').prop('selected', true);
        $('#Party').val("all");
    });

    $("#OfferPartyList").change(function () {
        var Value = $("#OfferPartyList").val();
        $('#Party').val(Value);
    });

    $("#AllQuantity").focusout(function () {
        $("#AllFreeQuantity").val($("#AllQuantity").val());
    });

    $("#AllIsFlexible").change(function () {
        var oldValue = $("#AllIsFlexible").val();
        if (oldValue != null && oldValue != "" && oldValue != undefined && oldValue == "N") {
            $("#AllFreeQuantity").prop("readonly", true);
        }
        else {
            $("#AllFreeQuantity").prop("readonly", false);
        }
    });

    //$("#ProductCode").focusout(function () {
    //    $(".preloader").show();
    //    var code = $("#ProductCode").val();
    //    $.ajax({
    //        url: "GetproductInfobyCode",
    //        type: 'POST',
    //        data: { "data": code },
    //        dataType: "json",
    //        success: function (objResult) {              
    //            if (objResult != null && objResult.length > 0) {
    //                $("#ProductName").val(objResult[0].ProductName);
    //            }
    //            else {
    //                OpenDialog("dialogMessage", "Product detail not found.", "false");
    //                $("#ProductCode").val("");                  
    //            }
    //            $(".preloader").hide();
    //        },
    //        error: function (xhr, data) {
    //            $(".preloader").hide();
    //            console.log(xhr);
    //            console.log("Error:", data);
    //        }
    //    });
    //});

    $("#AddProduct").click(function () {
        $("#AddProductdialog").dialog({
            modal: true,
            width: "60%"
        });
    });

    $("#AddBuyProduct").click(function () {
        $("#AddBuyProductdialog").dialog({
            modal: true,
            width: "60%"
        });
    });

    $("#AddAllProduct").click(function () {
        FreeProductArray = [];
        fillFreeProductGrid();
        $("#AddAllProductdialog").dialog({
            modal: true,
            width: "60%"
        });
    });

    $("#NoBtn").click(function () {
        $("#AddProductdialog").dialog('close');
    });

    $("#BuyNoBtn").click(function () {
        $("#AddBuyProductdialog").dialog('close');
    });

    $("#AllNoBtn").click(function () {
        $("#AddAllProductdialog").dialog('close');
    });


    $("#AllYesBtn").click(function () {
        var Qty = $("#AllQuantity").val();
        var FreeQty = $("#AllFreeQuantity").val();
        var IsFlexible = $("#AllIsFlexible").val();
        $(".preloader").show();
        $.ajax({
            type: 'POST',
            url: '/Product/GetProductList',
            success: function (resultData) {
                if (resultData != null) {
                    FreeProductArray = [];
                    for (i = 0; i < resultData.length; i++) {
                        if (resultData[i].IsActive) {
                            var product = { "ProdCode": resultData[i].ProductCode, "ProductName": resultData[i].ProductName, "Qty": Qty, "FreeQty": FreeQty, "Flexible": IsFlexible };
                            FreeProductArray.push(product);
                        }
                    }

                    fillFreeProductGrid();
                    $("#AddAllProductdialog").dialog('close');
                    $(".preloader").hide();

                    $("#btnSave").prop('disabled', false);
                }
                else {
                    $("#btnSave").prop('disabled', true);
                }
            },
            error: function (error) {
                $(".preloader").hide();
                alert(error);
            }

        });

        $("#btnSave").prop('disabled', false);
    });

    Date.prototype.addDays = function (days) {
        var date = new Date(this.valueOf());
        date.setDate(date.getDate() + days);
        return date;
    }

});

function SetSpecificCode(label, id) {
    var objIndex = FullProductList.findIndex((obj => obj.ProductName == label));
    if (objIndex != -1) {
        $("#" + id).val(parseFloat(FullProductList[objIndex].ProductCode));
    }
}

function resetform() {
    $('#CreateOfferForm')[0].reset();
    FreeProductArray = [];
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

function GetProductInfo(data) {
    $(".preloader").show();
    var dataValue = data;
    var IsBillOnMrp = true;
    if (dataValue != "" && dataValue != null && dataValue != undefined) {
        $.ajax({
            url: "GetproductInfobyCode",
            type: 'POST',
            data: { "data": dataValue },
            dataType: "json",
            success: function (objResult) {
                $(".preloader").hide();
                if (objResult != null && objResult.length > 0) {
                    fillProductValues(objResult[0]);
                }
                else {
                    OpenDialog("dialogMessage", "Product detail not found.", "false");
                    ClearProductdetails();
                }
            },
            error: function (xhr, data) {
                $(".preloader").hide();
                console.log(xhr);
                console.log("Error:", data);
            }
        });
    }
}

function GetOfferProduct() {
    $(".preloader").show();
    var dataValue = $("#OfferID").val();
    var IsBillOnMrp = true;
    if (dataValue != "" && dataValue != null && dataValue != undefined) {
        $.ajax({
            url: "GetSelectedOfferProducts",
            type: 'GET',
            async: false,
            data: { "OfferId": dataValue },
            dataType: "json",
            success: function (objResult) {
                $(".preloader").hide();
                FreeProductArray = [];
                if (objResult != null && objResult.length > 0) {
                    for (var i = 0; i < objResult.length; i++) {
                        FreeProductArray.push({ "ProdCode": objResult[i].ProdID, "ProductName": objResult[i].ProdName, "Qty": objResult[i].Qty, "FreeQty": objResult[i].FreeQty, "Flexible": objResult[i].IsFlexible, "OfferMrp": objResult[i].OfferMrp });
                    }
                }
                fillFreeProductGrid();
            },
            error: function (xhr, data) {
                $(".preloader").hide();
                console.log(xhr);
                console.log("Error:", data);
            }
        });
    }
}

function GetOfferBuyProduct() {
    $(".preloader").show();
    var dataValue = $("#OfferID").val();
    var IsBillOnMrp = true;
    if (dataValue != "" && dataValue != null && dataValue != undefined) {
        $.ajax({
            url: "GetOfferBuyProducts",
            type: 'GET',
            async: false,
            data: { "OfferId": dataValue },
            dataType: "json",
            success: function (objResult) {
                $(".preloader").hide();
                BuyProductArray = [];
                if (objResult != null && objResult.length > 0) {
                    for (var i = 0; i < objResult.length; i++) {
                        BuyProductArray.push({ "ProdCode": objResult[i].ProdID, "ProductName": objResult[i].ProdName, "Qty": objResult[i].Qty });
                    }
                }
                fillBuyProductGrid();
            },
            error: function (xhr, data) {
                $(".preloader").hide();
                console.log(xhr);
                console.log("Error:", data);
            }
        });
    }
}

function ClearProductdetails() {
    $("#ProductName").val("");
    $("#ProductCode").val("");
    $("#Batch").val("");
    $("#MRP").val("");
    $("#DP").val("");
    $("#BV").val("");
    $("#Qty").val("");
    $("#FreeQty").val("");
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

function Delete(ev) {
    FreeProductArray = $.grep(FreeProductArray, function (e) {
        return e.ProdCode != ev.data.record.ProdCode;
    });
    fillFreeProductGrid();
}

function BuyDelete(ev) {
    BuyProductArray = $.grep(BuyProductArray, function (e) {
        return e.ProdCode != ev.data.record.ProdCode;
    });
    fillBuyProductGrid();
}

function fillFreeProductGrid() {

    var offerType = $("#offerType").val();



    $("#noRecord").hide();
    if (FreeProductgrid != null) {
        FreeProductgrid.destroy();
        $('#FreeProductgrid').empty();
    }

    if (offerType == "1") {
        FreeProductgrid = $('#FreeProductgrid').grid({
            dataSource: FreeProductArray,
            uiLibrary: 'bootstrap',
            headerFilter: false,
            columns: [
                { field: 'ProdCode', title: 'Enter Product Code', width: 100, sortable: true, hidden: false, filterable: false },
                { field: 'ProductName', title: 'Product Name', width: 100, sortable: true, hidden: false, filterable: true },
                { field: 'Qty', title: 'Qty', width: 120, sortable: true, hidden: false, filterable: true },
                { field: 'Flexible', title: 'Is Flexible', width: 120, sortable: true, hidden: false, filterable: true },
                { field: 'FreeQty', title: 'FreeQty', width: 120, sortable: true, hidden: false, filterable: true },
                 { title: '', field: 'Delete', width: 34, type: 'icon', width: 70, icon: 'glyphicon-remove', tooltip: 'Edit', events: { 'click': Delete }, filterable: false }

            ],
            pager: { limit: 20, sizes: [15, 20, 35, 50, 65, 80, 95, 100] }
        });
    }
    else {
        FreeProductgrid = $('#FreeProductgrid').grid({
            dataSource: FreeProductArray,
            uiLibrary: 'bootstrap',
            headerFilter: false,
            columns: [
                { field: 'ProdCode', title: 'Enter Product Code', width: 100, sortable: true, hidden: false, filterable: false },
                { field: 'ProductName', title: 'Product Name', width: 100, sortable: true, hidden: false, filterable: true },
                { field: 'Qty', title: 'Qty', width: 120, sortable: true, hidden: false, filterable: true },
                { field: 'OfferMrp', title: 'Offer MRP', width: 120, sortable: true, hidden: false, filterable: true },
                 { title: '', field: 'Delete', width: 34, type: 'icon', width: 70, icon: 'glyphicon-remove', tooltip: 'Edit', events: { 'click': Delete }, filterable: false }
            ],
            pager: { limit: 20, sizes: [15, 20, 35, 50, 65, 80, 95, 100] }
        });
    }
}

function fillBuyProductGrid() {

    $("#noRecord").hide();
    if (BuyProductgrid != null) {
        BuyProductgrid.destroy();
        $('#BuyProductgrid').empty();
    }


    BuyProductgrid = $('#BuyProductgrid').grid({
        dataSource: BuyProductArray,
        uiLibrary: 'bootstrap',
        headerFilter: false,
        columns: [
            { field: 'ProdCode', title: 'Enter Product Code', width: 100, sortable: true, hidden: false, filterable: false },
            { field: 'ProductName', title: 'Product Name', width: 100, sortable: true, hidden: false, filterable: true },
            { field: 'Qty', title: 'Qty', width: 120, sortable: true, hidden: false, filterable: true },

             { title: '', field: 'Delete', width: 34, type: 'icon', width: 70, icon: 'glyphicon-remove', tooltip: 'Edit', events: { 'click': BuyDelete }, filterable: false }
        ],
        pager: { limit: 20, sizes: [15, 20, 35, 50, 65, 80, 95, 100] }
    });

}


