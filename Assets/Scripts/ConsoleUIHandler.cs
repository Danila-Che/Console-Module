using System;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(UIDocument))]
public sealed class ConsoleUIHandler : MonoBehaviour {
    private enum ConsoleStatus {
        Visible,
        Invisible,
    }

    private enum EnterStatus {
        ChooseCommand,
        ExecuteCommand,
        CommandNotFound,
    }

    [SerializeField] private bool _canListenDebugLogs;

    private ConsoleInput _input;
    private VisualElement _root;
    
    private Logs _logs;
    private CommandLine _commandLine;
    private CommandExecutor _commandExecutor;
    private Selector _selector;

    private ConsoleStatus _consoleStatus = ConsoleStatus.Invisible;
    private EnterStatus _enterStatus = EnterStatus.ExecuteCommand;

    private bool CanNotSubscribeDebugEvent => _canListenDebugLogs is false;

    private void Awake() {
        _input = new(GetComponent<PlayerInput>());
        _root = GetComponent<UIDocument>().rootVisualElement;
        _logs = new(_root.Q<VisualElement>("logs").Children().Cast<Label>());
        _commandLine = new(_root.Q<TextField>("input-command-field"));
        _commandExecutor = new(
            commandLine: _commandLine,
            logs: _logs,
            findObjectsOfType: FindObjectsOfType
        );
        _selector = new(
            hintsContainer: _root.Q<VisualElement>("hint-container"),
            upArrow: _root.Q<VisualElement>("up-arrow-hint"),
            downArrow: _root.Q<VisualElement>("down-arrow-hint"),
            hints: _root.Q<VisualElement>("hint-logs").Children().Cast<Label>().ToArray()
        );

        SetConsoleVisible();
        SubscribeToDebugEvent();

        _commandLine.OnCommandNameChanged += ReactOnNewCommand;
        _input.Switch.started += SwitchConsoleVisible;
        _input.Enter.started += HandleEnter;
        _input.Down.started += _selector.ChooseDownHint;
        _input.Up.started += _selector.ChooseUpHint;
        _selector.OnSelectedMethodChanged += ReactOnSelectedMethodChanges;
    }

    private void OnDestroy() {
        UnsubscribeToDebugEvent();

        _commandLine.OnCommandNameChanged -= ReactOnNewCommand;
        _input.Switch.started -= SwitchConsoleVisible;
        _input.Enter.started -= HandleEnter;
        _input.Down.started -= _selector.ChooseDownHint;
        _input.Up.started -= _selector.ChooseUpHint;
        _selector.OnSelectedMethodChanged -= ReactOnSelectedMethodChanges;
    }

    private void SubscribeToDebugEvent() {
        if (CanNotSubscribeDebugEvent) {
            return;
        }

        Application.logMessageReceived += _logs.CatchLog;
    }

    private void UnsubscribeToDebugEvent() {
        if (CanNotSubscribeDebugEvent) {
            return;
        }

        Application.logMessageReceived -= _logs.CatchLog;
    }

    private void ReactOnNewCommand(CommandFormat command) {
        if (command.HasNoName && command.HasNoModule) {
            _enterStatus = EnterStatus.CommandNotFound;
            _selector.Hide();
            return;
        }

        _enterStatus = EnterStatus.ChooseCommand;
        _selector.Show();
        _selector.Update(command, _commandExecutor.FindMatches(command));
    }

    private void SwitchConsoleVisible(InputAction.CallbackContext context) {
        _consoleStatus = _consoleStatus switch {
            ConsoleStatus.Visible => ConsoleStatus.Invisible,
            ConsoleStatus.Invisible => ConsoleStatus.Visible,
            _ => ConsoleStatus.Invisible,
        };

        SetConsoleVisible();
    }

    private void SetConsoleVisible() {
        _root.Q<VisualElement>("root").visible = _consoleStatus is ConsoleStatus.Visible;

        _commandLine.EmptyCommandInputField();
        _commandLine.Select();
        _selector.Hide();
    }

    private void HandleEnter(InputAction.CallbackContext context) {
        switch (_enterStatus) {
            case EnterStatus.ChooseCommand:
                ChooseOption();
                _enterStatus = EnterStatus.ExecuteCommand;
                break;
            case EnterStatus.ExecuteCommand:
                InputCommand();
                _enterStatus = EnterStatus.CommandNotFound;
                break;
        }
    }

    private void ChooseOption() {
        _selector.ChooseHint();
    }

    private void InputCommand() {
        _commandExecutor.Execute();
        _commandLine.EmptyCommandInputField();
        _selector.Hide();
    }

    private void ReactOnSelectedMethodChanges(Command command) {
        _commandLine.SetCommnadText(command.Name);
        _commandExecutor.SelectedMethod = command.MethodInfo;
    }
}

public struct Command {
    private MethodInfo _methodInfo;
    private string _module;
    private string _name;

    public Command(MethodInfo methodInfo, string module, string name)
    {
        _methodInfo = methodInfo;
        _module = module;
        _name = name;
    }

    public MethodInfo MethodInfo => _methodInfo;
    public string Module => _module;
    public string Name => _name;
}

public struct CommandFormat {
    private const StringSplitOptions SplitOptions = StringSplitOptions.RemoveEmptyEntries;

    private string _module;
    private string _name;
    private string[] _parameters;

    public CommandFormat(string commandText) {
        commandText = commandText.Trim();

        _module = GetModule(ref commandText);

        var lexemes = GetLexemes(commandText);

        if (lexemes.Any() is false) {
            _name = string.Empty;
            _parameters = null;
            return;
        }

        _name = lexemes.First();
        _parameters = lexemes.Skip(1).ToArray<string>();

        string GetModule(ref string commandText) {
            var parts = commandText.Split('.', 2, SplitOptions);
            
            if (parts.Length == 2) {
                commandText = parts[1].Trim();
                return parts[0].Trim();
            }

            if (parts.Length == 1) {
                if (commandText.Split(' ', SplitOptions).Length >= 2) {
                    return string.Empty;
                }

                return parts[0].Trim();
            }
            
            return string.Empty;
        }

        string[] GetLexemes(string commandText) {
            return commandText.Split(' ', SplitOptions).Select(line => line.Trim()).ToArray<string>();
        }
    }

    public string Module => _module;
    public string Name => _name;
    public string[] Parameters => _parameters;

    public bool HasNoName => string.IsNullOrEmpty(_name);
    public bool HasName => HasNoName is false;
    
    public bool HasParameters => _parameters is not null;
    public bool HasNoParameters => HasParameters is false;

    public bool HasNoModule => string.IsNullOrEmpty(_module);
    public bool HasModule => HasNoModule is false;

    public string ShowInfo() {
        var parameters = _parameters is not null ? string.Join(' ', _parameters) : "void";

        return $"MODULE: {_module} NAME: {_name} PARAMETER: {parameters}";
    }

    public override string ToString()
    {
        var parameters = _parameters is not null ? string.Join(' ', _parameters) : string.Empty;

        if (HasModule) {
            return $"{_module}.{_name} {parameters}";
        }

        return $"{_name} {parameters}";
    }
}

public interface ICommandLine
{
    CommandFormat Value { get; }
}

public interface ILogs {
    void AddLog(string text);
    void AddWarning(string text);
    void AddError(string text);
}

[Serializable]
public sealed class CommandLine: ICommandLine {
    public Action<CommandFormat> OnCommandNameChanged;

    private const string CommandMarker = "_";

    private readonly TextField _commandField;
    private string _lastModule = string.Empty;
    private string _lastName = string.Empty;

    public CommandLine(TextField commandField)
    {
        _commandField = commandField;

        _commandField.RegisterValueChangedCallback(callback => {
            var command = Value;
            var wasNameChanged = _lastName != command.Name;
            var wasModuleChanged = command.HasModule && _lastModule != command.Module;

            if (wasNameChanged || wasModuleChanged) {
                OnCommandNameChanged?.Invoke(command);
            }

            _lastModule = command.Module;
            _lastName = command.Name;
        });
    }

    public CommandFormat Value => new CommandFormat(_commandField.value);

    public void EmptyCommandInputField() {
        _commandField.SetValueWithoutNotify(CommandMarker);
        Select();
    }

    public void Select() {
        _commandField.Focus();
        _commandField.SelectAll();
    }

    public void SetCommnadText(string commandText) {
        _commandField.value = commandText;
        Select();
    }

    private void CursorToEnd() {
        var fromIndex = 5;
        var toIndex = 1;
        _commandField.SelectRange(fromIndex, toIndex);
        
        using (var evt = KeyboardEventBase<KeyDownEvent>.GetPooled('\0', KeyCode.LeftArrow, EventModifiers.FunctionKey))
        {
            _commandField.SendEvent(evt);
        }
    }
}

[Serializable]
public sealed class CommandExecutor {
    private struct Module {
        private Root _root;
        private IEnumerable<Type> _submodules;

        public Module(Root root)
        {
            _root = root;
            _submodules = root.GetLeafs();
        }

        public Root Root => _root;
        public Type RootType => _root.RootType;
        public IEnumerable<Type> Sumbmodules => _submodules;
    }

    private struct Root {
        public Type RootType;
        public IEnumerable<Leaf> Leafs;

        public Root(Type rootType, IEnumerable<Leaf> leafs)
        {
            RootType = rootType;
            Leafs = leafs;
        }

        public IEnumerable<Type> GetLeafs() {
            var result = Leafs.Select(leaf => leaf.Type);

            foreach (var leaf in Leafs) {
                result = result.Concat(leaf.GetLeafs());
            }

            return result.Distinct();
        }
    }

    private struct Leaf {
        public FieldInfo FieldInfo;
        public IEnumerable<Leaf> Leafs;

        public Leaf(FieldInfo fieldInfo) {
            FieldInfo = fieldInfo;
            Leafs = Enumerable.Empty<Leaf>();
        }

        public Leaf(FieldInfo fieldInfo, IEnumerable<Leaf> leafs)
        {
            FieldInfo = fieldInfo;
            Leafs = leafs;
        }

        public Type Type => FieldInfo.FieldType;

        public bool HasLeafs => Leafs.Any();
        public bool IsEnd => HasLeafs is false;

        public IEnumerable<Type> GetLeafs() {
            if (IsEnd) {
                return Enumerable.Empty<Type>().Append(FieldInfo.FieldType);
            }

            var result = Leafs.Select(leaf => leaf.FieldInfo.FieldType).Distinct();

            foreach (var leaf in Leafs) {
                result = result.Concat(leaf.GetLeafs());
            }

            return result.Distinct();
        }
    }

    private const BindingFlags FieldFlags =
          BindingFlags.Instance
        | BindingFlags.DeclaredOnly
        | BindingFlags.Public
        | BindingFlags.NonPublic
        | BindingFlags.Static;

    private const BindingFlags MethodFlags =
          BindingFlags.Instance
        | BindingFlags.DeclaredOnly
        | BindingFlags.InvokeMethod
        | BindingFlags.Public
        | BindingFlags.NonPublic
        | BindingFlags.Static;

    public delegate object[] FindObjectsOfType(Type type);

    private readonly ICommandLine _commandLine;
    private readonly ILogs _logs;
    private readonly FindObjectsOfType _findObjectsOfType;
    private readonly List<Command> _commands = new();
    private readonly List<Module> _modulesTree = new();

    public CommandExecutor(ICommandLine commandLine, ILogs logs, FindObjectsOfType findObjectsOfType) {
        _commandLine = commandLine;
        _logs = logs;
        _findObjectsOfType = findObjectsOfType;

        FindCommand();
        FindSubmodules();
    }

    public MethodInfo SelectedMethod { get; set; }

    public List<Command> FindMatches(CommandFormat command) {
        return _commands.FindAll(command => CheckModule(command) || CheckName(command));

        bool CheckModule (Command commandInfo) {
            return commandInfo.Module.Contains(command.Module);
        }

        bool CheckName (Command commandInfo) {
            return commandInfo.Name.Contains(command.Name);
        }
    }

    public void Execute() {
        var commandFormat = _commandLine.Value;

        if (SelectedMethod is null) {
            _logs.AddError($"Комманда не найдена: {commandFormat.Name}");
            return;
        }

        if (commandFormat.HasNoParameters) {
            InvokeSelectedMethod(null);
            return;
        }

        var parametersInfo = SelectedMethod.GetParameters();

        if (parametersInfo.Length != commandFormat.Parameters.Length) {
            _logs.AddError($"Некорректные параметры для {SelectedMethod.Name}");
            return;
        }

        var parameters = GetParsedParameters(parametersInfo, commandFormat.Parameters);

        if (parameters is null) {
            return;
        }

        InvokeSelectedMethod(parameters);
    }

    private object[] GetParsedParameters(ParameterInfo[] parametersInfo, string[] rawPrameters) {
        var length = parametersInfo.Length;
        var parameters = new object[length];

        for (int i = 0; i < length; i++) {
            var type = parametersInfo[i].ParameterType;
            var converter = TypeDescriptor.GetConverter(type);

            if(converter is not null)
            {
                try {
                    var parameter = converter.ConvertFromString(rawPrameters[i]);
                    parameters[i] = parameter;
                } catch {
                    _logs.AddError($"Некорректный параметр для {SelectedMethod.Name} [ожидалось {parametersInfo[i].Name}: {type} прибыло {rawPrameters[i]}]");
                    return null;
                }
            }
        }

        return parameters;
    }

    private void InvokeSelectedMethod(object[] parameters) {
        if (SelectedMethod.IsStatic) {
            SelectedMethod.Invoke(null, parameters);
            return;
        }

        var type = SelectedMethod.DeclaringType;

        if (type.IsSubclassOf(typeof(MonoBehaviour))) {
            foreach(var sceneObject in _findObjectsOfType(SelectedMethod.DeclaringType)) {
                SelectedMethod.Invoke(sceneObject, parameters);
            }

            return;
        }

        if (_modulesTree is null) {
            return;
        }

        if (_modulesTree.Any() is false) {
            return;
        }

        var roots = _modulesTree
            .Where(module => module.Sumbmodules.Contains(type))
            .Select(module => module.Root);

        if (roots.Any() is false) {
            var commandName = SelectedMethod.GetCustomAttribute<CommandAttribute>().Name;
            _logs.AddError(
                $"Класс {type}, реализующий команду {commandName}, не реализуется ни одним MonoBehaviour'ом, " +
                $"либо метод {SelectedMethod.Name} не является статическим");
            return;
        }
        
        foreach (var root in roots) {
            foreach(var sceneObject in _findObjectsOfType(root.RootType)) {
                foreach (var leaf in root.Leafs) {
                    InvokeMethodInTree(sceneObject, leaf);
                }
            }
        }

        void InvokeMethodInTree(object origin, Leaf leaf) {
            if (origin is null) {
                return;
            }

            var fieldOrigin = leaf.FieldInfo.GetValue(origin);

            if (fieldOrigin is null) {
                return;
            }

            if (leaf.Type == type) {
                SelectedMethod.Invoke(fieldOrigin, parameters);
            }

            foreach (var subleaf in leaf.Leafs) {
                InvokeMethodInTree(fieldOrigin, subleaf);
            }
        }
    }

    private void FindCommand() {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies) {
            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods(MethodFlags))
                .Where(m => m.GetCustomAttributes().OfType<CommandAttribute>().Any())
                .Select(m => {
                    var moduleAttribute = m.DeclaringType.GetCustomAttribute<ModuleAttribute>();
                    var commandAttribute = m.GetCustomAttribute<CommandAttribute>();

                    var moduleName = moduleAttribute is not null ? moduleAttribute.Name : string.Empty;
                    var commandName = commandAttribute is not null ? commandAttribute.Name : string.Empty;

                    return new Command(m, moduleName, commandName);
                });

            _commands.AddRange(methods);
        }
    }

    private void FindSubmodules() {
        var sceneObjects = _findObjectsOfType(typeof(MonoBehaviour));

        foreach (var sceneObject in sceneObjects) {
            var type = sceneObject.GetType();

            if (_modulesTree.Select(leaf => leaf.RootType).Contains(type)) {
                continue;
            }

            var submodules = GetSubmodules(type);

            if (submodules.Any() is false) {
                continue;
            }

            var leafs = Enumerable.Empty<Leaf>();

            foreach (var submodule in submodules) {
                leafs = leafs.Append(FindSubmodules(submodule));
            }

            _modulesTree.Add(new Module(new Root(type, leafs)));
        }
    }

    private Leaf FindSubmodules(FieldInfo origin) {
        var submodulesFieldTypes = GetSubmodules(origin.FieldType);

        var result = new Leaf(origin);

        foreach (var fieldType in submodulesFieldTypes) {
            result.Leafs = result.Leafs.Append(FindSubmodules(fieldType));
        }

        result.Leafs.Distinct();

        return result;
    }

    private IEnumerable<FieldInfo> GetSubmodules(Type type) {
        return type
            .GetFields(FieldFlags)
            .Where(field => field.GetCustomAttributes(false).OfType<SubmoduleAttribute>().Any())
            .Where(field => field.FieldType.IsSubclassOf(typeof(MonoBehaviour)) is false)
            .Distinct();
    }
}

[Serializable]
public sealed class Logs: ILogs {
    private enum LogStatus {
        Regular,
        Error,
        Warning,
    }

    private struct Log {
        private const string LogMarker = ">";

        private const string RegularStyle = "regular-log";
        private const string ErrorStyle = "error-log";
        private const string WarningStyle = "warning-log";

        private readonly Label _label;
        private LogStatus _status;

        public Log(Label label) {
            _label = label;
            _status = LogStatus.Regular;
        }

        public void SetMessage(string text, LogStatus status) {
            _label.text = GetLogMessage(text);
            SetStyle(status);
        }

        public void CopyAppearance(Log log) {
            _label.text = log._label.text;
            SetStyle(log._status);
        }

        private void SetStyle(LogStatus status) {
            DiscardStyle();
            _status = status;
            SetStyle();
        }

        private void DiscardStyle() {
            _label.RemoveFromClassList(GetStyle());
        } 

        private void SetStyle() {
            _label.AddToClassList(GetStyle());
        }

        private string GetStyle() {
            return _status switch {
                LogStatus.Error => ErrorStyle,
                LogStatus.Warning => WarningStyle,
                _ => RegularStyle,
            };
        }

        private string GetLogMessage(string text) {
            return $"{LogMarker} {text}";
        }
    }

    private readonly Log[] _logs;
    private int _logCount = 0;

    public Logs(IEnumerable<Label> logs)
    {
        _logs = logs.Select(log => new Log(log)).ToArray();
    }

    private int MaxLogCount => _logs.Length - 1;

    public void AddLog(string text) {
        ScrollUpLogs();
        _logs[0].SetMessage(text, LogStatus.Regular);
    }

    public void AddError(string text) {
        ScrollUpLogs();
        _logs[0].SetMessage(text, LogStatus.Error);
    }

    public void AddWarning(string text) {
        ScrollUpLogs();
        _logs[0].SetMessage(text, LogStatus.Warning);
    }

    public void CatchLog(string condition, string stackTrace, LogType type) {
        switch (type) {
            case LogType.Error:
            case LogType.Exception:
                AddError(condition);
                break;
            case LogType.Warning:
                AddWarning(condition);
                break;
            case LogType.Assert:
            case LogType.Log:
            default:
                AddLog(condition);
                break;
        }
    }

    private void ScrollUpLogs() {
        for (int i = _logCount; i >= 1; i--) {
            _logs[i].CopyAppearance(_logs[i - 1]);
        }

        _logCount = Math.Clamp(_logCount + 1, 0, MaxLogCount);
    }
}

[Serializable]
public sealed class Selector {
    public Action<Command> OnSelectedMethodChanged;

    private const string HighlightedHintStyle = "regular-hint-highlighted";
    private const string EndOfVoidMethodHint = " void";

    private readonly VisualElement _hintsContainer;
    private readonly VisualElement _upArrow;
    private readonly VisualElement _downArrow;
    private readonly Label[] _hints;

    private List<Command> _possibleOptions = new();
    private int _numberOfHighlightedHint;
    private int _startPoint = 0;

    public Selector(VisualElement hintsContainer, VisualElement upArrow, VisualElement downArrow, Label[] hints)
    {
        _hintsContainer = hintsContainer;
        _upArrow = upArrow;
        _downArrow = downArrow;
        _hints = hints;
    }

    private bool HasTopLimit => _startPoint is 0;
    private bool HasNoTopLimit => HasTopLimit is false;

    private bool CanHaveLimits => _possibleOptions.Count > _hints.Length;

    private bool HasBottomLimit => _startPoint == _possibleOptions.Count - _hints.Length;
    private bool HasNoBottomLimit => HasBottomLimit is false;

    public void Show() {
        _hintsContainer.visible = true;
        ResetHintsUI();
    }

    public void Hide() {
        _hintsContainer.visible = false;
        ResetHintsUI();
    }

    public void Update(CommandFormat command, List<Command> possibleOptions) {
        _possibleOptions = possibleOptions;
        var hasNoOption = _possibleOptions.Any() is false;
        var incorrectCommandName = command.HasNoName;

        if (hasNoOption || incorrectCommandName) {
            Hide();
            return;
        }

        ResetHintsUI();
        UpdateHintsUI();
    }

    public void ChooseDownHint(InputAction.CallbackContext context) {
        if (_possibleOptions.Any() is false) {
            return;
        }

        ReturnToRegularHint();
        MoveOptionListOffset();
        DownHint();
        UpdateHintsUI();
    }

    public void ChooseUpHint(InputAction.CallbackContext context) {
        if (_possibleOptions.Any() is false) {
            return;
        }

        ReturnToRegularHint();
        MoveOptionListOffset();
        UpHint();
        UpdateHintsUI();
    }

    public void ChooseHint() {
        var optionNumber = _startPoint + _numberOfHighlightedHint;
        var commandName = _hints[_numberOfHighlightedHint].text;

        OnSelectedMethodChanged?.Invoke(_possibleOptions[optionNumber]);
        PrintSelectedHint(commandName);
    }

    private void ResetHintsUI() {
        EmptyHints();
        ReturnToRegularHint();
        DisableArrows();

        _startPoint = 0;
        _numberOfHighlightedHint = 0;
    }

    private void UpdateHintsUI() {
        PrintHints();
        SetArrows();
        HighlightHint();
    }

    private void PrintHints() {
        for (int i = 0; i < _hints.Length && i < _possibleOptions.Count; i++) {
            _hints[i].text = GetHintMessage(_possibleOptions[i + _startPoint]);
        }
    }

    private string GetHintMessage(Command option) {
        var parameters = option.MethodInfo.GetParameters().Select(parameter => GetFormatedParameter(parameter)).ToArray();
        var hintText = new StringBuilder(
              option.Module.Length
            + 1
            + option.Name.Length
            + parameters.Length
            * 15
            + EndOfVoidMethodHint.Length);

        if (string.IsNullOrEmpty(option.Module) is false) {
            hintText.Append($"{option.Module}.");
        } 

        hintText.Append(option.Name);

        if (parameters.Any()) {
            hintText.Append(' ');
        } else {
            hintText.Append(EndOfVoidMethodHint);
        }

        hintText.AppendJoin(' ', parameters);

        return hintText.ToString();
    }

    private string GetFormatedParameter(ParameterInfo parameter) {
        return $"{parameter.Name}: {parameter.ParameterType.ToString().Replace("System.", string.Empty)}";
    }

    private void EmptyHints() {
        foreach (var hint in _hints) {
            hint.text = string.Empty;
        }
    }

    private void SetArrows() {
        var showTopArrow = HasNoTopLimit && CanHaveLimits;
        var showBottomArrow = HasNoBottomLimit && CanHaveLimits;

        _upArrow.visible = showTopArrow;
        _downArrow.visible = showBottomArrow;
    }

    private void DisableArrows() {
        _upArrow.visible = false;
        _downArrow.visible = false;
    }

    private void ReturnToRegularHint() {
        _hints[_numberOfHighlightedHint].RemoveFromClassList(HighlightedHintStyle);
    }

    private void HighlightHint() {
        _hints[_numberOfHighlightedHint].AddToClassList(HighlightedHintStyle);
    }

    private void DownHint() {
        _numberOfHighlightedHint++;
        ClampNumberOfHighlightedHint();
    }

    private void UpHint() {
        _numberOfHighlightedHint--;
        ClampNumberOfHighlightedHint();
    }

    private void MoveOptionListOffset() {
        if (_hints.Length >= _possibleOptions.Count) {
            return;
        }

        if (_numberOfHighlightedHint <= 0) {
            _startPoint--;
        }

        if (_numberOfHighlightedHint >= _hints.Length - 1) {
            _startPoint++;
        }

        _startPoint = Math.Clamp(_startPoint, 0, _possibleOptions.Count - _hints.Length);
    }

    private void ClampNumberOfHighlightedHint() {
        _numberOfHighlightedHint = Math.Clamp(_numberOfHighlightedHint, 0, _hints.Length - 1);
        _numberOfHighlightedHint = Math.Clamp(_numberOfHighlightedHint, 0, _possibleOptions.Count - 1);
    }

    private void PrintSelectedHint(string text) {
        ResetHintsUI();
        _possibleOptions.Clear();
        _hints.First().text = text;
    }
}

public sealed class ConsoleInput {
    private InputAction _switch;
    private InputAction _enter;
    private InputAction _down;
    private InputAction _up;

    public ConsoleInput(PlayerInput input) {
        _switch = input.actions.FindAction("Switch");
        _enter = input.actions.FindAction("CommandEnter");
        _down = input.actions.FindAction("MoveDown");
        _up = input.actions.FindAction("MoveUp");
    }

    public InputAction Switch => _switch;
    public InputAction Enter => _enter;
    public InputAction Down => _down;
    public InputAction Up => _up;
}
