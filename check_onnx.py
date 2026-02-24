import onnxruntime as ort
import numpy as np
import joblib

scaler = joblib.load("scaler.pkl")
label_map = joblib.load("label_map.pkl")
inv_label_map = {v: k for k, v in label_map.items()}

session = ort.InferenceSession("gesture_model.onnx")

# Load REAL saved sample
sample = np.load("DATA/boar/boar_0.npy")  # change sign to test
sample = sample.reshape(1, -1).astype(np.float32)

sample = scaler.transform(sample)

outputs = session.run(None, {"input": sample})
prediction = np.argmax(outputs[0])

print("Predicted:", inv_label_map[prediction])
