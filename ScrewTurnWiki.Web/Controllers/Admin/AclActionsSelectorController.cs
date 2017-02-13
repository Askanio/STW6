using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.Web.Code;
using ScrewTurn.Wiki.Web.Models.Admin.Common;

namespace ScrewTurn.Wiki.Web.Controllers.Admin
{
    [RoutePrefix("Admin")]
    public class AclActionsSelectorController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AclActionsSelectorController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public AclActionsSelectorController(ApplicationSettings settings) : base(settings)
        {
        }

        [ChildActionOnly]
        public ActionResult GetAclActionsSelector(AclResources res, string[] grantedActions, string[] deniedActions)
        {
            var model = new AclActionsSelectorModel();
            FillActions(model, res);

            if (grantedActions == null) throw new ArgumentNullException(nameof(grantedActions));
            if (deniedActions == null) throw new ArgumentNullException(nameof(deniedActions));

            SelectActions(model, grantedActions, deniedActions);

            return PartialView("~/Views/Admin/Common/AclActionsSelector.cshtml", model);
        }

        /// <summary>
        /// Fill action's lists
        /// </summary>
        private void FillActions(AclActionsSelectorModel model, AclResources res)
        {
            string[] temp = null;
            switch (res)
            {
                case AclResources.Globals:
                    var currentWiki = Tools.DetectCurrentWiki();
                    if (Settings.GetEnableAdditionalsGlobalPermissions(currentWiki))
                        temp = Actions.ForGlobals.All;
                    else
                        temp = Actions.ForGlobals.Base;
                    break;
                case AclResources.Namespaces:
                    temp = Actions.ForNamespaces.All;
                    break;
                case AclResources.Pages:
                    temp = Actions.ForPages.All;
                    break;
                case AclResources.Directories:
                    temp = Actions.ForDirectories.All;
                    break;
                default:
                    throw new NotSupportedException("ACL Resource not supported");
            }

            // Add full-control action
            string[] actions = new string[temp.Length + 1];
            actions[0] = Actions.FullControl;
            Array.Copy(temp, 0, actions, 1, temp.Length);

            model.ActionsGrant = new List<SelectListItem>();
            model.ActionsDeny = new List<SelectListItem>();
            foreach (string action in actions)
            {
                SelectListItem item = new SelectListItem() {Text = GetName(res, action), Value = action};
                model.ActionsGrant.Add(item);
                SelectListItem itemBlank = new SelectListItem() {Text = "&nbsp;", Value = action};
                model.ActionsDeny.Add(itemBlank);
            }
        }

        private string GetName(AclResources res, string action)
        {
            switch (res)
            {
                case AclResources.Globals:
                    return Actions.ForGlobals.GetFullName(action);
                case AclResources.Namespaces:
                    return Actions.ForNamespaces.GetFullName(action);
                case AclResources.Pages:
                    return Actions.ForPages.GetFullName(action);
                case AclResources.Directories:
                    return Actions.ForDirectories.GetFullName(action);
                default:
                    throw new NotSupportedException("ACL Resource not supported");
            }
        }

        private void SelectActions(AclActionsSelectorModel model, string[] granted, string[] denied)
        {
            // Deselect and enable all items first
            foreach (SelectListItem item in model.ActionsGrant)
            {
                item.Selected = false;
                item.Disabled = false;
            }
            foreach (SelectListItem item in model.ActionsDeny)
            {
                item.Selected = false;
                item.Disabled = false;
            }

            // Select specific ones
            foreach (SelectListItem item in model.ActionsGrant)
            {
                item.Selected = Array.Find(granted, s => s == item.Value) != null;
            }
            foreach (SelectListItem item in model.ActionsDeny)
            {
                item.Selected = Array.Find(denied, s => s == item.Value) != null;
            }

            SetupCheckBoxes(model, null);
        }

        private void SetupCheckBoxes(AclActionsSelectorModel model, IList<SelectListItem> list)
        {
            // Setup the checkboxes so that full-control takes over the others,
            // and there cannot be an action that is both granted and denied
            // The list parameter determines the last checkbox list that changed status,
            // allowing to switch the proper checkbox pair
            if (model.ActionsGrant.Count > 0)
            {
                if (list == null) list = model.ActionsGrant;
                IList<SelectListItem> other = list == model.ActionsGrant ? model.ActionsDeny : model.ActionsGrant;

                // Verify whether full-control is checked
                // If so, disable all other checkboxes
                for (int i = 1; i < list.Count; i++)
                {
                    if (list[0].Selected)
                    {
                        list[i].Selected = false;
                        list[i].Disabled = true;
                        other[i].Disabled = false;
                    }
                    else
                    {
                        list[i].Disabled = false;
                    }
                }

                // Switch status of other list checkboxes
                for (int i = 0; i < other.Count; i++)
                {
                    if (i > 0 && other[0].Selected)
                    {
                        other[i].Selected = false;
                        other[i].Disabled = true;
                    }
                    else
                    {
                        if (!other[i].Disabled && !list[i].Disabled && list[i].Selected)
                        {
                            other[i].Selected = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of the actions.
        /// </summary>
        public static string[] GetActions(IList<SelectListItem> actions)
        {
            List<string> actionsResult = new List<string>();
            foreach (SelectListItem item in actions)
                if (item.Selected) actionsResult.Add(item.Value);
            return actionsResult.ToArray();
        }
    }
}