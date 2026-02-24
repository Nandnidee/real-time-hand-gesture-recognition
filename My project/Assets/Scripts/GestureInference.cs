using UnityEngine;
using Unity.Barracuda;

public class GestureInference : MonoBehaviour
{
    public NNModel modelAsset;

    private Model runtimeModel;
    private IWorker worker;

    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }

    public int Predict(float[] landmarks)
    {
        float[] normalized = NormalizeLandmarks(landmarks);

        Tensor input = new Tensor(1, 126, normalized);
        worker.Execute(input);

        Tensor output = worker.PeekOutput();
        int predicted = output.ArgMax()[0];

        input.Dispose();
        output.Dispose();

        return predicted;
    }

    float[] NormalizeLandmarks(float[] data)
    {
        float[] output = new float[126];

        for (int hand = 0; hand < 2; hand++)
        {
            int baseIndex = hand * 63;

            float wristX = data[baseIndex + 0];
            float wristY = data[baseIndex + 1];
            float wristZ = data[baseIndex + 2];

            float maxVal = 0f;

            // subtract wrist
            for (int i = 0; i < 21; i++)
            {
                int idx = baseIndex + i * 3;

                float x = data[idx] - wristX;
                float y = data[idx + 1] - wristY;
                float z = data[idx + 2] - wristZ;

                output[idx] = x;
                output[idx + 1] = y;
                output[idx + 2] = z;

                maxVal = Mathf.Max(maxVal,
                    Mathf.Abs(x),
                    Mathf.Abs(y),
                    Mathf.Abs(z));
            }

            // scale
            if (maxVal > 0)
            {
                for (int i = 0; i < 21; i++)
                {
                    int idx = baseIndex + i * 3;
                    output[idx] /= maxVal;
                    output[idx + 1] /= maxVal;
                    output[idx + 2] /= maxVal;
                }
            }
        }

        return output;
    }

    private void OnDestroy()
    {
        worker.Dispose();
    }
}
