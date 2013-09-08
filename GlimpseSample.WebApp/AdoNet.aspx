<%@ Page Title="" Language="C#" MasterPageFile="~/GlimpseSample.Master" AutoEventWireup="true" CodeBehind="AdoNet.aspx.cs" Inherits="GlimpseSample.WebApp.AdoNet" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    ADO.NET
</asp:Content>
<asp:Content ContentPlaceHolderID="Content" runat="server">
    <asp:GridView runat="server" ID="gridUsers" AutoGenerateColumns="False">
        <Columns>
            <asp:BoundField HeaderText="ID" DataField="Id" />
            <asp:BoundField HeaderText="User Name" DataField="Name" />
        </Columns>
    </asp:GridView>
</asp:Content>