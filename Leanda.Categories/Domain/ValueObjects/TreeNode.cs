using Newtonsoft.Json;
using Sds.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public static class EnumerableTreeNodeExtensions
    {
        public static List<TreeNode> ApplyIdsToNodes(this List<TreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                if(node.Id == default(Guid))
                {
                    node.Id = Guid.NewGuid();
                }
                
                if (node.Children != null && node.Children.Any())
                {
                    node.Children = ApplyIdsToNodes(node?.Children);
                }
            }
            return nodes;
        }

        public static IEnumerable<Guid> GetNodeIds(this List<TreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Id != default(Guid))
                {
                    yield return node.Id;
                }

                if (node.Children != null && node.Children.Any())
                {
                    var enumer = GetNodeIds(node?.Children).GetEnumerator();
                    while (enumer.MoveNext())
                    {
                        yield return enumer.Current;
                    }
                }
            }
        }

        public static bool ContainsTree(this List<TreeNode> nodes, Guid id)
        {
            foreach (var node in nodes)
            {
                if (node.Id == id)
                {
                    return true;
                }
                return ContainsTree(node.Children, id);
            }
            return false;
        }
    }
}
