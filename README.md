# Hand Gesture Recognition (Python)

## Overview

This project performs real-time hand gesture recognition using a webcam.

The system:
1. Captures hand landmarks using MediaPipe  
2. Saves landmark data for dataset creation  
3. Trains an MLP classifier  
4. Converts the trained model to ONNX format  
5. Validates ONNX output before Unity integration  

https://github.com/user-attachments/assets/c1cbd154-defc-406b-bdcd-f462a998d23a

Supported gestures:
- Korean Heart
- Like
- Dislike
- Victory

---

## Project Workflow

### 1️⃣ Data Collection
`hand_tracking.py`

- Detects hand using MediaPipe  
- Extracts 21 landmarks (x, y, z)  
- Saves 63 features per sample  
- Builds dataset for training  

---

### 2️⃣ Model Training
`training_mlp.py`

- Loads collected landmark dataset  
- Trains MLP classifier  
- Saves trained model  

---

### 3️⃣ Convert to ONNX
`convert_to_onnx.py`

- Converts trained model to ONNX format  
- Enables cross-platform usage (Unity integration)  

---

### 4️⃣ Validate ONNX Model
`check_onnx.py`

- Loads ONNX model  
- Verifies predictions match original model  
- Ensures correct export  

---

## Actual Project Structure

```
SIGN_DETECTION/
│
├── DATA/                  # Saved landmark dataset
│
├── hand_tracking.py       # Data collection script
├── training_mlp.py        # Model training
├── convert_to_onnx.py     # Convert model to ONNX
├── check_onnx.py          # Validate ONNX output
│
├── requirements.txt
├── .gitignore
└── README.md
```

## Installation

```bash
git clone https://github.com/Nandnidee/real-time-hand-gesture-recognition.git
cd sign_detection
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
```

---

## Run Instructions

### Collect Data (run only if you want any new sign to append)
```bash
python hand_tracking.py
```

### Train Model
```bash
python training_mlp.py
```

### Convert to ONNX
```bash
python convert_to_onnx.py
```

### Validate ONNX
```bash
python check_onnx.py
```

---
## Author

Deepika  
AI / Computer Vision Project
