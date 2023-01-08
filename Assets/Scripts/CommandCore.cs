using System;

[AttributeUsage(AttributeTargets.Method)] 
public sealed class CommandAttribute: Attribute {
    private string _name;

    public CommandAttribute() {
        _name = string.Empty;
    }

    public CommandAttribute(string name) {
        _name = name;
    }

    public string Name => _name;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)] 
public sealed class ModuleAttribute: Attribute {
    private string _name;

    public ModuleAttribute() {
        _name = string.Empty;
    }

    public ModuleAttribute(string name) {
        _name = name;
    }

    public string Name => _name;
}

[AttributeUsage(AttributeTargets.Field)] 
public sealed class SubmoduleAttribute: Attribute {
    public SubmoduleAttribute() { }
}

public sealed class CommandCore
{

}
