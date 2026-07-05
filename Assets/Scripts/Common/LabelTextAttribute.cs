using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class LabelTextAttribute : PropertyAttribute
{
    public string Text { get; }

    public LabelTextAttribute(string text)
    {
        Text = text;
    }
}
