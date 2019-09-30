using Newtonsoft.Json;
using Sds.Domain;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain.ValueObjects
{
    public class TreeNode : ValueObject<TreeNode>
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<TreeNode> Children { get; set; }

        public TreeNode(Guid id, string title, List<TreeNode> children = null)
        {
            Id = id;
            Title = title;
            Children = children;
        }

        public TreeNode()
        {
        }

        public TreeNode(string title, List<TreeNode> children = null)
        {
            Title = title;
            Children = children;
        }

        protected override IEnumerable<object> GetAttributesToIncludeInEqualityCheck()
        {
            return new List<Object>() { Id, Title };
        }
    }
}
