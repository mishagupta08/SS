<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SCAPPResponse.aspx.cs" Inherits="SCAPPResponse" %>

 <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Response</title>
      
</head>
<body>
    <form id="form1" runat="server">
     <div>
    <asp:Label ID="lbl_ResponseCode" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_ResponseMessage" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_DateCreated" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_PaymentID" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_MerchantRefNo" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_Amount" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_Mode" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_BillingName" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_BillingAddress" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_BillingCity" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_BillingState" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_BillingPostalCode" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_BillingCountry" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_BillingPhone" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_BillingEmail" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_DeliveryName" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_DeliveryAddress" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_DeliveryCity" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_DeliveryState" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_DeliveryPostalCode" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_DeliveryCountry" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_DeliveryPhone" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_Description" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_IsFlagged" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_TransactionID" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_PaymentMethod" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_RequestID" runat="server" Visible="false"></asp:Label>
    <asp:Label ID="lbl_SecureHash" runat="server" Visible="false"></asp:Label>
     <div>
       <asp:Label runat="server" ID="lblError" ForeColor="Red"></asp:Label>
    </div>
    </div>
    </form>
</body>
</html>
