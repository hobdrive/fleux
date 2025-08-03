using System;
using System.Collections.Generic;

namespace Fleux.Core;

public abstract class RuntimeEffect
{
    protected string Name;
    protected Dictionary<string, object> Parameters;

    public static int MS_TO_TICKS = 10000; // 1 millisecond = 10,000 ticks
    public long EffectStart { get; protected set; } = 0;
    public long EffectDuration { get; protected set; } = 1000 * MS_TO_TICKS;

    public bool IsDisposed { get; private set; } = false;

    public RuntimeEffect(string name, Dictionary<string, object> parameters)
    {
        Name = name;
        Parameters = parameters;
    }

    public virtual void Start()
    {
        EffectStart = DateTime.Now.Ticks;
    }

    public virtual bool Process()
    {
        if (DateTime.Now.Ticks - EffectStart > EffectDuration)
        {
            IsDisposed = true;
            return false;
        }
        return true;
    }

    public virtual object GetState()
    {
        return null;
    }
}