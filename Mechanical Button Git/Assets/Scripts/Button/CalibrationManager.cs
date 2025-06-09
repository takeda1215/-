using UnityEngine;
using TMPro; // TextMeshPro �p�̖��O��Ԃ�ǉ�

public class CalibrationManager : MonoBehaviour
{
    public SerialReceiver serialReceiver;   // SerialReceiver ����f�[�^���󂯎��
    public TMP_Text displayText;            // TextMeshPro �p�� UI

    public float scalingFactor = 0.002f;    // ���̃X�P�[�����O�W���i��Œ����j
    public float offsetValue = 0.0f;        // �I�t�Z�b�g�␳�l

    void Update()
    {
        if (serialReceiver != null && !string.IsNullOrEmpty(serialReceiver.receivedData))
        {
            string processedData = serialReceiver.receivedData.Replace("Weight: ", "").Trim();
            if (float.TryParse(processedData, out float parsedData))
            {
                // �I�t�Z�b�g�l��K�p
                parsedData += offsetValue;

                // ���̃X�P�[�����O�W�����|���Ď��ʂ��v�Z
                float massInKg = parsedData * scalingFactor;

                // ���ʂ���͂��v�Z
                float forceInNewton = massInKg * 9.81f;

                // UI �ɕ\��
                displayText.text = $"Raw Data: {parsedData}\nMass (kg): {massInKg}\nForce (N): {forceInNewton}";
            }
        }
    }
}
