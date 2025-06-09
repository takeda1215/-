using System;
using UnityEngine;

[Serializable]
public class ButtonModel
{
    public enum ButtonType { Keyboard, RemoteControl, Mouse, Sponge, TouchPanel }

    public ButtonType buttonType;
    public float thresholdForce;     // N
    public float hardness;          // N/m
    public float maxDisplacement;   // m
    public float activationKg = 0f; // 指を出すトリガー（kg）

    // 実際に外部からこのメソッドを呼び出して「kg」を渡す
    public void UpdateState(float kg, float scalingFactor)
    {
        float force = kg * scalingFactor * 9.81f;
        bool prevPressed = IsPressed;
        float prevDisp = Displacement;

        if (buttonType == ButtonType.Sponge || buttonType == ButtonType.TouchPanel)
        {
            Displacement = Mathf.Clamp(-force / hardness, maxDisplacement, 0f);
            IsPressed = Mathf.Abs(Displacement) > 0.001f;
        }
        else
        {
            IsPressed = force >= thresholdForce;
            Displacement = IsPressed ? maxDisplacement : 0f;
        }

        // 指表示トリガー
        if (kg >= activationKg && !FingerShown)
        {
            FingerShown = true;         // ← ここを追加
            OnFingerShow?.Invoke();
        }
        else if (kg < activationKg && FingerShown)
        {
            FingerShown = false;        // ← ここを追加
            OnFingerHide?.Invoke();
        }

        // 押下／解放イベント
        if (IsPressed && !prevPressed) OnPressed?.Invoke();
        if (!IsPressed && prevPressed) OnReleased?.Invoke();
        // 変位変化イベント
        if (Mathf.Abs(Displacement - prevDisp) > 0.00001f)
            OnDisplacementChanged?.Invoke(Displacement);
    }

    // 状態
    public bool IsPressed { get; private set; }
    public float Displacement { get; private set; }
    public bool FingerShown { get; private set; }

    // イベント
    public event Action OnPressed;
    public event Action OnReleased;
    public event Action<float> OnDisplacementChanged;
    public event Action OnFingerShow;
    public event Action OnFingerHide;
}
