using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using RedirectManager.Backend.Models;
using RedirectManager.Extensions;
using RedirectManager.Models;

namespace RedirectManager.Backend.Controllers
{
    [PluginController("RDM")]
    public class RedirectsController : UmbracoAuthorizedJsonController
    {
        private readonly IRedirectUrlService _redirectUrlService;
        private IContentTypeService _contentTypeService;
        private readonly IDomainService _domainService;
        private readonly UmbracoHelper _umbracoHelper;

        public RedirectsController(IRedirectUrlService redirectUrlService, 
            UmbracoHelper umbracoHelper, 
            IContentTypeService contentTypeService,
            IDomainService domainService)
        {
            _redirectUrlService = redirectUrlService;
            _umbracoHelper = umbracoHelper;
            _contentTypeService = contentTypeService;
            _domainService = domainService;
        }

        [HttpGet]
        public IEnumerable<HomeNodeView> GetAllHomePages()
        {
            return _umbracoHelper.ContentAtRoot().Where(x => x.ContentType.Alias == "home").Select(x => 
                new HomeNodeView { Id = x.Id, Name = x.Name ?? "Unnamed Home Page" });
        }

        [HttpGet]
        public ActionResult<IEnumerable<NodeDetailView>> GetNodeChildren(int id)
        {
            var foundNode = _umbracoHelper.Content(id);
            if (foundNode == null)
            {
                //throw new Exception(string.Format("Node with Id {0} is null.", id));
                return BadRequest($"Node with Id {id} is null.");
            }
            var nodeChildren = foundNode.Children;
            if (nodeChildren == null)
            {
                //throw new Exception(string.Format("Node with Id {0} has no children.", id));
                return BadRequest($"Node with Id {id} has no children.");
            }

            return Ok(nodeChildren
                .Where(x => x.IsValidRedirectPage())
                .Select(x =>
                    new NodeDetailView { 
                        Id = x.Id, 
                        Name = x.Name ?? "Unnamed Node", 
                        Level = x.Level,
                        Key = x.Key,
                        NodeType = x.ContentType.Alias,
                        HasChildren = x.Children?.Count() > 0,
                        Icon = _contentTypeService.Get(x.ContentType.Id)?.Icon ?? "icon-help-alt"
                    })
                );
        }

        public ActionResult<IEnumerable<NodeDetailView>> GetNodeImmediateAncestors(int id, int levels = 2)
        {
            IPublishedContent? foundNode = _umbracoHelper.Content(id);
            if (foundNode == null)
            {
                //throw new Exception(string.Format("Node with Id {0} is null.", id));
                return BadRequest($"Node with Id {id} is null.");
            }

            IEnumerable<IPublishedContent>? ancestors = new List<IPublishedContent>();
            for (var i = 0; i < levels; i++)
            {
                foundNode = foundNode.Parent;
                if (foundNode != null && foundNode.Level >= 1)
                {
                    ancestors = foundNode.Children();
                }
                else
                {
                    return BadRequest($"Ancestors could not be found at this level or requested level is too far up the content tree");
                }
            }

            if (ancestors == null || !ancestors.Any())
            {
                //throw new Exception(string.Format("Node with Id {0} has no children.", id));
                return BadRequest($"Node with Id {id} has no children.");
            }

            return Ok(ancestors
                .Where(x => x.IsValidRedirectPage())
                .Select(x =>
                    new NodeDetailView
                    {
                        Id = x.Id,
                        Name = x.Name ?? "Unnamed Node",
                        Level = x.Level,
                        Key = x.Key,
                        NodeType = x.ContentType.Alias,
                        HasChildren = x.Children?.Count() > 0,
                        Icon = _contentTypeService.Get(x.ContentType.Id)?.Icon ?? "icon-help-alt"
                    })
                );
        }


        [HttpPost]
        public IActionResult RegisterRedirect([FromBody] RegisterRedirectCommand registerRedirect)
        {
            if (!Uri.TryCreate(registerRedirect.OldUrl, UriKind.Absolute, out Uri uriResult))
            {
                //throw new Exception(string.Format("Provided old URL {0} is invalid.", uriResult));
                return BadRequest($"Provided old URL {registerRedirect.OldUrl} has an invalid schema.");
            }

            var allDomains = _domainService.GetAll(true).ToList();
            var domain = allDomains?
                .FirstOrDefault(f => f.DomainName == uriResult.Authority
                                     || f.DomainName == $"https://{uriResult.Authority}"
                                     || f.DomainName == $"http://{uriResult.Authority}"
                                     || f.DomainName == $"https://{uriResult.Authority}/"
                                     || f.DomainName == $"http://{uriResult.Authority}/");

            if (domain == null)
            {
                //throw new Exception(string.Format("Provided old URL {0} is not in tennancy.", uriResult.Authority));
                return BadRequest($"Provided old URL authority {uriResult.Authority} is not in tennancy.");
            }
            var SiteId = domain.RootContentId;

            string validatedOldUrl = uriResult.AbsolutePath;
            if (uriResult.AbsolutePath.EndsWith(@"/"))
            {
                validatedOldUrl = uriResult.AbsolutePath.Substring(0, uriResult.AbsolutePath.LastIndexOf('/'));
            }

            var existingRedirects = _redirectUrlService.SearchRedirectUrls($"{SiteId}{validatedOldUrl}", 0, 10, out long resultCount);
            if (resultCount > 0)
            {
                var existing = existingRedirects.Select(x => x.ContentKey).FirstOrDefault();
                var existingRedirectUrl = _umbracoHelper.Content(existing);
                return BadRequest($"A redirect for this url already exists to {existingRedirectUrl?.Url()}");
            }

            _redirectUrlService.Register($"{SiteId}{validatedOldUrl}", registerRedirect.DestinationGuid, "");

            return Ok("New Redirect has been registered");
        }
    }
}
