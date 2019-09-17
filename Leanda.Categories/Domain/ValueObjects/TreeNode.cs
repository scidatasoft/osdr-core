using Sds.Domain;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain.ValueObjects
{
    public class TreeNode : ValueObject<TreeNode>
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public List<TreeNode> Children { get; set; }

        public TreeNode(Guid id, string title, List<TreeNode> children)
        {
            Id = id;
            Title = title;
            Children = children;
        }

        protected override IEnumerable<object> GetAttributesToIncludeInEqualityCheck()
        {
            return new List<Object>() { Id };
        }
    }
}
