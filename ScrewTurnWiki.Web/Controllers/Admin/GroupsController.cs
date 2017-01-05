using System.Collections.Generic;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Code.Attributes;
using ScrewTurn.Wiki.Web.Code.Providers;
using ScrewTurn.Wiki.Web.Localization.Messages;
using ScrewTurn.Wiki.Web.Models.Admin.Common;
using ScrewTurn.Wiki.Web.Models.Admin.Groups;

namespace ScrewTurn.Wiki.Web.Controllers.Admin
{
    [RoutePrefix("Admin")]
    [CheckActionForGlobals(Action = CheckActionForGlobalsAttribute.ActionForGlobals.ManageGroups)]
    public class GroupsController : BaseController
    {
        private const string TempDataResultText = "ResultText";

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupsController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public GroupsController(ApplicationSettings settings) : base(settings)
        {
        }

        [HttpGet]
        [Route("UserGroups")]
        public ActionResult UserGroups()
        {
            var model = new UserGroupsModel();
            base.PrepareModel(model, AdminMenu.UserGroups);
            model.Groups = GetUserGroups("");

            model.NewGroupButtonEnable = ProviderHelper.GetProviders(CurrentWiki, ProviderType.Users,
                UsersProviderIntendedUse.GroupsManagement, true).Count > 0;

            var resultText = TempData[TempDataResultText];
            if (resultText != null)
            {
                model.ResultText = resultText as string;
                model.ResultCss = "resultok";
            }

            return View("~/Views/Admin/Groups/UserGroups.cshtml", model);
        }

        private List<UserGroupRow> GetUserGroups(string currentUserGroupName)
        {
            List<UserGroup> allGroups = Users.GetUserGroups(CurrentWiki);
            List<UserGroupRow> result = new List<UserGroupRow>(allGroups.Count);
            foreach (UserGroup group in allGroups)
                result.Add(new UserGroupRow(group, group.Name == currentUserGroupName));
            return result;
        }

        private bool GetActionsSelectorVisible()
        {
            string currentUser = SessionFacade.GetCurrentUsername();
            string[] currentGroups = SessionFacade.GetCurrentGroupNames(CurrentWiki);
            return base.CanManagePermissions(currentUser, currentGroups);         
        }

        [HttpGet]
        [Route("CreateUserGroup")]
        public ActionResult CreateUserGroup()
        {
            var model = new EditUserGroupModel();
            base.PrepareModel(model, AdminMenu.UserGroups);

            if (GetActionsSelectorVisible())
            {
                model.ActionsSelectorVisible = true;
                model.ActionsGrant = new string[0];
                model.ActionsDeny = new string[0];
            }
            model.GroupNameEnabled = true;
            model.DescriptionEnabled = true;

            model.Providers = ProviderHelper.GetProviders(CurrentWiki, ProviderType.Users,
                UsersProviderIntendedUse.GroupsManagement, true);

            model.CreateUserGroup = true;
            model.ButtonSaveVisible = true;

            return View("~/Views/Admin/Groups/EditUserGroup.cshtml", model);
        }

        [HttpPost]
        [Route("SaveUserGroup")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveUserGroup(EditUserGroupModel model, AclActionsSelectorModel selectorModel)
        {
            model.ResultCss = "";
            model.ResultText = "";

            model.ActionsGrant = AclActionsSelectorController.GetActions(selectorModel.ActionsGrant);
            model.ActionsDeny = AclActionsSelectorController.GetActions(selectorModel.ActionsDeny);

            if (ModelState.IsValid)
            {
                model.GroupName = (model.CurrentGroupName ?? model.GroupName).Trim();

                if (model.CreateUserGroup)
                {
                    Log.LogEntry("Group creation requested for " + model.GroupName, EntryType.General, SessionFacade.CurrentUsername, CurrentWiki);

                    // Add the new group then set its global permissions
                    bool done = Users.AddUserGroup(CurrentWiki, model.GroupName, model.Description,
                        Collectors.CollectorsBox.UsersProviderCollector.GetProvider(model.SelectedProvider, CurrentWiki));

                    if (done)
                    {
                        UserGroup currentGroup = null;
                        currentGroup = Users.FindUserGroup(CurrentWiki, model.GroupName);
                        done = AddAclEntries(currentGroup, model.ActionsGrant, model.ActionsDeny);

                        if (done)
                        {
                            //model.ResultCss = "resultok";
                            TempData[TempDataResultText] = Messages.GroupCreated;
                            return RedirectToAction("UserGroups");
                        }

                        model.ResultCss = "resulterror";
                        model.ResultText = Messages.GroupCreatedCouldNotStorePermissions;
                    }
                    else
                    {
                        model.ResultCss = "resulterror";
                        model.ResultText = Messages.CouldNotCreateGroup;
                    }
                }
                else
                {
                    Log.LogEntry("Group update requested for " + model.GroupName, EntryType.General, SessionFacade.CurrentUsername, CurrentWiki);

                    UserGroup currentGroup = Users.FindUserGroup(CurrentWiki, model.GroupName);

                    // Perform proper actions based on provider read-only settings
                    // 1. If possible, modify group
                    // 2. Update ACLs

                    bool done = true;

                    if (!currentGroup.Provider.UserGroupsReadOnly)
                    {
                        done = Users.ModifyUserGroup(currentGroup, model.Description);
                    }

                    if (done)
                    {
                        done = RemoveAllAclEntries(currentGroup);
                        if (done)
                        {
                            done = AddAclEntries(currentGroup, model.ActionsGrant, model.ActionsDeny);

                            if (done)
                            {
                                //model.ResultCss = "resultok";
                                TempData[TempDataResultText] = Messages.GroupUpdated;
                                return RedirectToAction("UserGroups");
                            }

                            model.ResultCss = "resulterror";
                            model.ResultText = Messages.GroupSavedCouldNotStoreNewPermissions;
                        }
                        else
                        {
                            model.ResultCss = "resulterror";
                            model.ResultText = Messages.GroupSavedCouldNotDeleteOldPermissions;
                        }                        
                    }
                    else
                    {
                        model.ResultCss = "resulterror";
                        model.ResultText = Messages.CouldNotUpdateGroup;
                    }

                }

            }

            if (model.CreateUserGroup)
            {
                model.GroupNameEnabled = true;
                model.DescriptionEnabled = true;
                model.ButtonSaveVisible = true;
                model.ActionsSelectorVisible = true;

                base.PrepareModel(model, AdminMenu.UserGroups);

                model.Providers = ProviderHelper.GetProviders(CurrentWiki, ProviderType.Users,
                    UsersProviderIntendedUse.GroupsManagement, true);
            }
            else
            {
                UserGroup group = Users.FindUserGroup(CurrentWiki, model.CurrentGroupName);
                PrepareEditUserModel(model, group);
                model.ActionsSelectorVisible = GetActionsSelectorVisible();
            }

            return View("~/Views/Admin/Groups/EditUserGroup.cshtml", model);
        }

        [HttpGet]
        [Route("EditUserGroup")]
        public ActionResult EditUserGroup(string groupName)
        {
            var model = new EditUserGroupModel();

            UserGroup group = Users.FindUserGroup(CurrentWiki, groupName);

            // Select group's global permissions
            if (GetActionsSelectorVisible())
            {
                model.ActionsSelectorVisible = true;
                AuthReader authReader = new AuthReader(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
                model.ActionsGrant = authReader.RetrieveGrantsForGlobals(group);
                model.ActionsDeny = authReader.RetrieveDenialsForGlobals(group);
            }

            PrepareEditUserModel(model, group);

            model.CurrentGroupName = group.Name;

            var resultText = TempData[TempDataResultText];
            if (resultText != null)
            {
                model.ResultText = resultText as string;
                model.ResultCss = "resulterror";
            }
            
            return View("~/Views/Admin/Groups/EditUserGroup.cshtml", model);
        }

        private void PrepareEditUserModel(EditUserGroupModel model, UserGroup group)
        {
            base.PrepareModel(model, AdminMenu.UserGroups);

            model.Providers = ProviderHelper.GetProviders(CurrentWiki, ProviderType.Users,
                UsersProviderIntendedUse.GroupsManagement, true);

            model.GroupName = group.Name;
            model.GroupNameEnabled = false;
            model.Description = group.Description;
            model.SelectedProvider = group.Provider.GetType().FullName;

            model.CreateUserGroup = false;
            model.ButtonSaveVisible = true;
            model.ButtonDeleteVisible = true;
            bool isDefaultGroup =
                group.Name == Settings.GetAdministratorsGroup(CurrentWiki) ||
                group.Name == Settings.GetUsersGroup(CurrentWiki) ||
                group.Name == Settings.GetAnonymousGroup(CurrentWiki);

            // Enable/disable interface sections based on provider read-only settings
            model.DescriptionEnabled = !group.Provider.UserGroupsReadOnly;
            model.ButtonDeleteVisible = !group.Provider.UserGroupsReadOnly && !isDefaultGroup;
        }

        [HttpGet]
        [Route("DeleteUserGroup")]
        public ActionResult DeleteUserGroup(string groupName)
        {
            Log.LogEntry("Group deletion requested for " + groupName, EntryType.General, SessionFacade.CurrentUsername, CurrentWiki);

            UserGroup currentGroup = Users.FindUserGroup(CurrentWiki, groupName);

            if (!currentGroup.Provider.UserGroupsReadOnly)
            {
                // Remove all global permissions for the group then delete it
                bool done = RemoveAllAclEntries(currentGroup);
                if (done)
                {
                    done = Users.RemoveUserGroup(CurrentWiki, currentGroup);

                    if (done)
                    {
                        TempData[TempDataResultText] = Messages.GroupDeleted;
                        //lblResult.CssClass = "resultok";
                        return RedirectToAction("UserGroups");
                    }

                    TempData[TempDataResultText] = Messages.PermissionsDeletedCouldNotDeleteGroup;
                    //model.ResultCss = "resulterror";
                }
                else
                {
                    //model.ResultCss = "resulterror";
                    TempData[TempDataResultText] = Messages.CouldNotDeletePermissions;
                }
            }
            return RedirectToAction("EditUserGroup", new { groupName });
        }

        /// <summary>
        /// Removes all the ACL entries for a group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns><c>true</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        private bool RemoveAllAclEntries(UserGroup group)
        {
            AuthWriter authWriter = new AuthWriter(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            return authWriter.RemoveEntriesForGlobals(group);
        }

        /// <summary>
        /// Adds some ACL entries for a group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="grants">The granted actions.</param>
        /// <param name="denials">The denied actions.</param>
        /// <returns><c>true</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        private bool AddAclEntries(UserGroup group, string[] grants, string[] denials)
        {
            AuthWriter authWriter = new AuthWriter(Collectors.CollectorsBox.GetSettingsProvider(CurrentWiki));
            foreach (string action in grants)
            {
                bool done = authWriter.SetPermissionForGlobals(AuthStatus.Grant, action, group);
                if (!done) return false;
            }

            foreach (string action in denials)
            {
                bool done = authWriter.SetPermissionForGlobals(AuthStatus.Deny, action, group);
                if (!done) return false;
            }

            return true;
        }


    }
}