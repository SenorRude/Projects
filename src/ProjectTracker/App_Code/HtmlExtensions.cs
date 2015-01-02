using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ProjectTracker
{
    public static class HtmlExtensions
    {
        private static IHtmlString Concat(this IHtmlString first, params IHtmlString[] strings)
        {
            return MvcHtmlString.Create(first.ToString() + string.Concat(strings.Select(s => s.ToString())));
        }

        private static IHtmlString Concat(this IHtmlString first, params string[] strings)
        {
            return MvcHtmlString.Create(first.ToString() + string.Concat(strings.Select(s => s.ToString())));
        }

        public static IHtmlString Concat(params IHtmlString[] strings)
        {
            return MvcHtmlString.Create(string.Concat(strings.Select(s => s.ToString())));
        }

        public static HtmlString ToHtmlString(this TagBuilder tag)
        {
            return new HtmlString(tag.ToString());
        }

        // Builds URL by finding the best matching route that corresponds to the current URL,
        // with given parameters added or replaced.
        public static string ActionWithParams(this UrlHelper url, string actionName, object substitutions)
        {
            // Add query string params to the route value dictionary.
            // NameValueCollection.ToString() doesn't return a proper query string. However,
            // HttpUtility.ParseQueryString() actually returns an HttpValueCollection (private type), and
            // HttpValueCollection.ToString() DOES return a proper query string.
            NameValueCollection qs = System.Web.HttpUtility.ParseQueryString(
                url.RequestContext.HttpContext.Request.QueryString.ToString());

            // Add/remove/override parameters we're changing
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(substitutions.GetType()))
            {
                string value = property.GetValue(substitutions).ToString();
                if (string.IsNullOrEmpty(value))

                    qs.Remove(property.Name);
                else
                    qs[property.Name] = value;
            }

            //UrlHelper will find the first matching route
            //(the routes are searched in the order they were registered).
            //The unmatched parameters will be added as query string.
            if (!string.IsNullOrWhiteSpace(qs.ToString()))
                return url.Action(actionName) + "?" + qs.ToString();
            else
                return url.Action(actionName);
        }
    }
}