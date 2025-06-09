using UnityEngine;
using TMPro; // TextMeshPro 用の名前空間を追加

public class CalibrationManager : MonoBehaviour
{
    public SerialReceiver serialReceiver;   // SerialReceiver からデータを受け取る
    public TMP_Text displayText;            // TextMeshPro 用の UI

    public float scalingFactor = 0.002f;    // 仮のスケーリング係数（後で調整）
    public float offsetValue = 0.0f;        // オフセット補正値

    void Update()
    {
        if (serialReceiver != null && !string.IsNullOrEmpty(serialReceiver.receivedData))
        {
            string processedData = serialReceiver.receivedData.Replace("Weight: ", "").Trim();
            if (float.TryParse(processedData, out float parsedData))
            {
                // オフセット値を適用
                parsedData += offsetValue;

                // 仮のスケーリング係数を掛けて質量を計算
                float massInKg = parsedData * scalingFactor;

                // 質量から力を計算
                float forceInNewton = massInKg * 9.81f;

                // UI に表示
                displayText.text = $"Raw Data: {parsedData}\nMass (kg): {massInKg}\nForce (N): {forceInNewton}";
            }
        }
    }
}
