using UnityEngine;

[Module("engine")]
public sealed class Engine : MonoBehaviour
{
    [Submodule]
    private readonly Piston _piston1 = new(1);
    [Submodule]
    private readonly Piston _piston2 = new(2);
    [Submodule]
    private readonly SparkPlug _sparkPlug = new();

    public void SayHello() {

    }

    [Command("turn_on")]
    private void TurnOn() {
        Debug.Log("The engine was turned on");
    }

    [Command("turn_on")]
    private void TurnOn(int number) {
        Debug.Log($"The engine was turned on -> {number}");
    }

    [Command("turn_off")]
    private void TurnOff() {
        Debug.Log("The engine was turned off");
    }

    [Command("turn_off")]
    private void TurnOff(int number) {
        Debug.Log($"The engine was turned off -> {number}");
    }

    [Command("show_information")]
    private static void PrintInformation() {
        Debug.LogWarning("This engine is V8");
    }
}

public sealed class Piston {
    private readonly int _number;
    [Submodule()]
    private readonly Detail _detail1 = new();
    [Submodule()]
    private readonly Detail _detail2 = new();

    public Piston(int number)
    {
        _number = number;
    }

    [Command("move")]
    private void Move() {
        Debug.Log($"Move piston #{_number}");
    }
}

public sealed class SparkPlug {
    [Submodule()]
    private readonly Detail _detail = new();
}

public sealed class Detail {
    [Submodule]
    private readonly DetailInfo _detailInfo = new();
}

public sealed class DetailInfo {
    [Command("show_information")]
    private void PrintInfo() {
        Debug.Log("Dies ist ein Detail");
    }
}

public sealed class Car {
    [Command("move")]
    private void Move() {
        Debug.LogError("Move");
    }

    [Command("turn_on")]
    private void TurnOn(string text) {
        Debug.Log($"The car was turned on -> {text}");
    }

    [Command("turn_off")]
    private void TurnOff(string text) {
        Debug.Log($"The car was turned off -> {text}");
    }
}
