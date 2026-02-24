import torch
import torch.nn as nn

actions = ['dil', 'dislike', 'like', 'celebrate']

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

model = MLP(num_classes=len(actions))
model.load_state_dict(torch.load("gesture_model.pth"))
model.eval()

dummy_input = torch.randn(1, 126)

torch.onnx.export(
    model,
    dummy_input,
    "gesture_model.onnx",
    input_names=["input"],
    output_names=["output"],
    opset_version=11
)

print("Clean ONNX exported successfully.")
