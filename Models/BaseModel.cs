using System;
using ActionTimelineReplacement.Interfaces;
using Dalamud.Interface.Utility.Raii;

namespace ActionTimelineReplacement.Models;

public abstract class BaseModel<TData>(TData data) : IBaseModel
{
    protected TData Data = data;
    
    public TData Value => Data;

    public void Draw()
    {
        using (ImRaii.PushId(GetHashCode()))
        {
            if (!DrawImplementation()) return;
        }

        Changed();
        OnChanged?.Invoke();
        Service.Model.Save();
    }
    
    public static implicit operator TData(BaseModel<TData> item) => item.Data;

    /// <summary>
    /// Returns true if the item was changed.
    /// </summary>
    /// <returns></returns>
    protected abstract bool DrawImplementation();

    public virtual void Changed()
    {
    }

    public event Action? OnChanged;
}