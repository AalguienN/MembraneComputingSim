using System.Collections.Generic;

namespace MCSim.Utils;

public class TreeNode<T>
{
    public T Value { get; set; }
    public List<TreeNode<T>> Children { get; set; }

    public TreeNode(T value)
    {
        Value = value;
        Children = new List<TreeNode<T>>();
    }
    public void AddChild(TreeNode<T> child)
    {
        Children.Add(child);
    }
}