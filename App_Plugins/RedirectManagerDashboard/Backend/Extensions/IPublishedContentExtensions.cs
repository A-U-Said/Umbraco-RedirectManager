using Umbraco.Cms.Core.Models.PublishedContent;

namespace RedirectManager.Extensions
{
    public static class IPublishedContentExtensions
    {
        public static bool IsValidRedirectPage(this IPublishedContent node)
        {
            return
                node.ContentType.Alias != "siteSettings" &&
                node.ContentType.Alias != "errorPage" &&
                node.ContentType.Alias != "searchPage" &&
                node.ContentType.Alias != "home" &&
                node.ContentType.Alias != "contacts";
        }
    }
}
