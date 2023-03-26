using System;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace RedirectManager.Models
{
    public class NodeDetailView : HomeNodeView
    {
        public int Level { get; set; }
        public Guid Key { get; set; }
        public string NodeType { get; set; }
        public bool HasChildren { get; set; }
        public string Icon { get; set; }
    }
}
