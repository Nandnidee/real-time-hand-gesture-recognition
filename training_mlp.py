import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
import os
from sklearn.model_selection import train_test_split
from torch.utils.data import DataLoader, TensorDataset
from collections import Counter

# ========= CONFIG =========
DATA_PATH = "DATA"
actions = ['dil', 'dislike', 'like', 'celebrate']
BATCH_SIZE = 32
EPOCHS = 70
LEARNING_RATE = 0.001
# ==========================

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

X = []
y = []

label_map = {label: i for i, label in enumerate(actions)}

# -------- Normalization Function --------
def normalize_landmarks(sample):
    if sample.shape[0] != 126:
        raise ValueError(f"Expected 126 features, got {sample.shape}")

    sample = sample.reshape(2, 21, 3)

    for hand in range(2):
        wrist = sample[hand][0]
        sample[hand] = sample[hand] - wrist

        max_value = np.max(np.abs(sample[hand]))
        if max_value > 0:
            sample[hand] = sample[hand] / max_value

    return sample.flatten()

# -------- Load Data --------
for action in actions:
    folder = os.path.join(DATA_PATH, action)

    if not os.path.exists(folder):
        continue

    for file in os.listdir(folder):
        if file.endswith(".npy"):
            file_path = os.path.join(folder, file)
            data = np.load(file_path)

            try:
                data = normalize_landmarks(data)
                X.append(data)
                y.append(label_map[action])
            except Exception as e:
                print(f"Skipping {file_path} | Error: {e}")

X = np.array(X)
y = np.array(y)

print("Total samples:", len(X))
print("Class distribution:", Counter(y))

if len(X) == 0:
    raise ValueError("No training data found!")

# -------- Train/Test Split --------
X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.2, stratify=y, random_state=42
)

X_train = torch.tensor(X_train, dtype=torch.float32).to(device)
y_train = torch.tensor(y_train, dtype=torch.long).to(device)

X_test = torch.tensor(X_test, dtype=torch.float32).to(device)
y_test = torch.tensor(y_test, dtype=torch.long).to(device)

train_loader = DataLoader(
    TensorDataset(X_train, y_train),
    batch_size=BATCH_SIZE,
    shuffle=True
)

test_loader = DataLoader(
    TensorDataset(X_test, y_test),
    batch_size=BATCH_SIZE,
    shuffle=False
)

# -------- Model --------
class MLP(nn.Module):
    def __init__(self, num_classes):
        super().__init__()
        self.model = nn.Sequential(
            nn.Linear(126, 128),
            nn.ReLU(),

            nn.Linear(128, 64),
            nn.ReLU(),

            nn.Linear(64, num_classes)
        )

    def forward(self, x):
        return self.model(x)

model = MLP(num_classes=len(actions)).to(device)

criterion = nn.CrossEntropyLoss()
optimizer = optim.Adam(model.parameters(), lr=LEARNING_RATE)

# -------- Training --------
for epoch in range(EPOCHS):
    model.train()
    running_loss = 0

    for batch_X, batch_y in train_loader:
        optimizer.zero_grad()
        outputs = model(batch_X)
        loss = criterion(outputs, batch_y)
        loss.backward()
        optimizer.step()

        running_loss += loss.item()

    avg_loss = running_loss / len(train_loader)
    print(f"Epoch [{epoch+1}/{EPOCHS}]  Loss: {avg_loss:.4f}")

# -------- Evaluation --------
model.eval()
correct = 0
total = 0

with torch.no_grad():
    for batch_X, batch_y in test_loader:
        outputs = model(batch_X)
        preds = torch.argmax(outputs, dim=1)
        correct += (preds == batch_y).sum().item()
        total += batch_y.size(0)

accuracy = correct / total
print("Test Accuracy:", accuracy)

# -------- Save Model --------
torch.save(model.state_dict(), "gesture_model.pth")
print("Model saved as gesture_model.pth")