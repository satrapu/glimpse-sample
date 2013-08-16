<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GlimpseSample.WebApp.Default" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Glimpse Sample Application</title>
    </head>
    <body>
        <h1>Welcome to the Glimpse Sample Application</h1>
        <form runat="server">
            <asp:GridView runat="server" ID="gridUsers" AutoGenerateColumns="False">
                <AlternatingRowStyle />
                <Columns>
                    <asp:BoundField HeaderText="ID" DataField="Id" />
                    <asp:BoundField HeaderText="User Name" DataField="Name" />
                </Columns>
            </asp:GridView>
        </form>
    </body>
</html>