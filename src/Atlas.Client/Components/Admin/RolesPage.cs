﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Models.Admin.Roles;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Atlas.Client.Components.Admin
{
    public abstract class RolesPage : AdminPageBase
    {
        protected IndexPageModel Model { get; set; }

        protected string EditTitle => IsEdit ? Loc["Edit Role"] : Loc["Create New Role"];
        protected string EditButton => IsEdit ? Loc["Update"] : Loc["Save"];

        protected bool DisplayForm { get; set; }
        protected bool DisplayMembers { get; set; }
        protected bool Loading { get; set; }
        protected bool IsEdit { get; set; }
        protected string DeleteId { get; set; }
        protected string UserIdToRemove { get; set; }
        protected string RoleForUsers { get; set; }

        protected override async Task OnInitializedAsync()
        {
            DisplayForm = false;
            DisplayMembers = false;
            IsEdit = false;
            Model = await ApiService.GetFromJsonAsync<IndexPageModel>("api/admin/roles/list");
        }

        protected async Task NewAsync()
        {
            DisplayForm = true;
            DisplayMembers = false;
            IsEdit = false;
            Model.EditRole.Id = null;
            Model.EditRole.Name = null;
            await JsRuntime.InvokeVoidAsync("scrollToTarget", "form");
        }

        protected async Task EditAsync(string id, string name)
        {
            DisplayForm = true;
            DisplayMembers = false;
            IsEdit = true;
            Model.EditRole.Id = id;
            Model.EditRole.Name = name;
            await JsRuntime.InvokeVoidAsync("scrollToTarget", "form");
        }

        protected async Task SaveAsync()
        {
            var action = IsEdit ? "update" : "create";
            await ApiService.PostAsJsonAsync($"api/admin/roles/{action}", Model.EditRole);
            await OnInitializedAsync();
        }

        protected void Cancel()
        {
            DisplayForm = false;
            DisplayMembers = false;
            IsEdit = false;
            Model.EditRole.Id = null;
            Model.EditRole.Name = null;
        }

        protected async Task DeleteAsync(MouseEventArgs e)
        {
            await ApiService.DeleteAsync($"api/admin/roles/delete/{DeleteId}");
            await OnInitializedAsync();
        }

        protected void SetDeleteId(string id)
        {
            DeleteId = id;
        }

        protected void SetUseIdToRemove(string id)
        {
            UserIdToRemove = id;
        }

        protected async Task LoadUsersInRole(string roleName)
        {
            
            Loading = true;
            DisplayForm = false;
            DisplayMembers = true;
            RoleForUsers = roleName;
            await JsRuntime.InvokeVoidAsync("scrollToTarget", "users");
            Model.UsersInRole = await ApiService.GetFromJsonAsync<IList<IndexPageModel.UserModel>>($"api/admin/roles/users-in-role/{roleName}");
            Loading = false;
        }

        protected async Task RemoveUserFromRoleAsync()
        {
            Loading = true;
            await ApiService.DeleteAsync($"api/admin/roles/remove-user-from-role/{UserIdToRemove}/{RoleForUsers}");
            await LoadUsersInRole(RoleForUsers);
        }
    }
}