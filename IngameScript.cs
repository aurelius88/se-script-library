
using Sandbox.ModAPI.Ingame;
using System;

public abstract class IngameScript
{
    protected IMyGridTerminalSystem GridTerminalSystem;
    protected string Storage = "";
    protected IMyProgrammableBlock Me;
    protected Action<string> Echo;
    protected TimeSpan ElapsedTime;

    public abstract void Main();

}

