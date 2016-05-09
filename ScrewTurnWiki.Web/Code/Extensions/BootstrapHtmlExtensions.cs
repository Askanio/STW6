using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ScrewTurn.Wiki.Web.Code.Extensions
{
    /// <summary>
    /// Extension methods that spit out Bootstrap class="" into the elements.
    /// </summary>
    public static class BootstrapHtmlExtensions
    {
        public static MvcHtmlString BootstrapTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false, int tabIndex = 0)
        {
            return htmlHelper.TextBoxFor(expression, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
        }

        public static MvcHtmlString BootstrapDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string help, int tabIndex = 0)
        {
            return htmlHelper.DropDownListFor(expression, selectList, new { @class = "form-control", rel = "popover", data_content = help, tabindex = tabIndex });
        }

        private static object GetHtmlAttributes(string help, bool autoCompleteOff, int tabIndex, string additionalCssClass = "")
        {
            if (autoCompleteOff)
                return new { @class = "form-control" + additionalCssClass, rel = "popover", data_content = help, tabIndex = tabIndex, autocomplete = "off" };
            else
                return new { @class = "form-control" + additionalCssClass, rel = "popover", data_content = help, tabIndex = tabIndex };
        }
    }
}