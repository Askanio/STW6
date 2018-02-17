using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace ScrewTurn.Wiki.Web.Code.Extensions
{
    public static class HtmlExtensions
    {
        public static IHtmlString WikiTextBoxFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            object htmlAttributes,
            bool disabled
            )
        {
            var attributes = new RouteValueDictionary(htmlAttributes);
            if (disabled)
            {
                attributes["disabled"] = "disabled";
            }
            return htmlHelper.TextBoxFor(expression, attributes);
        }

        public static MvcHtmlString WikiDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList,
            object htmlAttributes, bool disabled)
        {
            var attributes = new RouteValueDictionary(htmlAttributes);
            if (disabled)
            {
                attributes["disabled"] = "disabled";
            }
            return htmlHelper.DropDownListFor(expression, selectList, attributes);
        }

        public static IHtmlString WikiCheckBoxFor<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, bool>> expression,
            object htmlAttributes,
            bool disabled
            )
        {
            var attributes = new RouteValueDictionary(htmlAttributes);
            if (disabled)
            {
                attributes["disabled"] = "disabled";
            }
            return htmlHelper.CheckBoxFor(expression, attributes);
        }
    }
}