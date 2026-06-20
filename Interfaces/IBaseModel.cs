using System;

namespace ActionTimelineReplacement.Interfaces;

public interface IBaseModel : IDrawItem
{
    void Changed();
    
    event Action? OnChanged;
}