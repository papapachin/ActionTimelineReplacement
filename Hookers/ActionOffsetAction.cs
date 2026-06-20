using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ActionTimelineReplacement.Models;

namespace ActionTimelineReplacement.Hookers;

public sealed class ActionOffsetAction
{
    private delegate IntPtr GetActionDataDelegate(uint actionId);

    private static GetActionDataDelegate GetActionDataHook => field ??=
        Marshal.GetDelegateForFunctionPointer<GetActionDataDelegate>(
            Service.Scanner.ScanText("E8 ?? ?? ?? ?? F6 40 3E 10"));

    private short? _defaultValue;
    private readonly uint _id;
    private readonly List<ActionOffsetModel> _models = [];
    private ActionOffsetModel? _activeModel;
    public ActionOffsetDefinition Definition { get; }

    private ActionOffsetAction(ActionOffsetDefinition definition, uint id)
    {
        Definition = definition;
        _id = id;
    }

    private static readonly Dictionary<uint, Dictionary<ActionOffsetDefinition, ActionOffsetAction>> Dictionary = new();

    public static ActionOffsetAction GetOrCreate(ActionOffsetDefinition definition, uint id)
    {
        ref var subDictionary = ref CollectionsMarshal.GetValueRefOrAddDefault(Dictionary, id, out var exists);
        if (subDictionary is null || !exists)
        {
            subDictionary = new();
        }

        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault(subDictionary, definition, out exists);
        if (result is null || !exists)
        {
            result = new ActionOffsetAction(definition, id);
        }

        return result;
    }

    private unsafe short* ValuePointer
    {
        get
        {
            var newValue = (short*)(GetActionDataHook(_id) + Definition.Offset);
            if ((IntPtr)field == IntPtr.Zero)
            {
                field = newValue;
            }
            else if (newValue != field)
            {
                Service.Log.Error(
                    "The Short Pointer was changed, so it is impossible to safe the pointer as a field!");
                field = newValue;
            }

            return field;
        }
    }

    public unsafe short DefaultValue
    {
        get
        {
            if (_defaultValue.HasValue)
            {
                return _defaultValue.Value;
            }

            _defaultValue = *ValuePointer;
            // Another way is getting from the sheet.
            // var action = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(_id);
            // _defaultValue = action.ExcelPage.ReadUInt16(action.RowOffset + _definition.Offset);
            return _defaultValue.Value;
        }
    }

    public void RegisterModel(ActionOffsetModel model)
    {
        _models.Add(model);
    }

    public void UnregisterModel(ActionOffsetModel model)
    {
        if (_models.Remove(model))
        {
            UpdatePriorityOrEnable();
        }
    }

    public void UpdatePriorityOrEnable()
    {
        var lastModel = _activeModel;
        UpdateActiveModel();
        if (lastModel == _activeModel) return;

        SetValue(_activeModel?.Value ?? DefaultValue);
    }

    public void UpdateValue(ActionOffsetModel model)
    {
        if (_activeModel is null)
        {
            UpdateActiveModel();
        }

        if (_activeModel == model)
        {
            SetValue(model.Value);
        }
    }

    private void UpdateActiveModel()
    {
        _activeModel = _models
            .Where(m => m.Enable)
            .MaxBy(m => m.Priority);
    }

    private unsafe void SetValue(short value)
    {
        if (_defaultValue is null)
        {
            _defaultValue = *ValuePointer;
        }

        Service.Log.Debug(
            "Set the Field[{FieldName}] with Value[{Value}] in the Action[{ActionID}].",
            Definition.Name,
            value,
            _id.ToString());

        *ValuePointer = value;
    }
}