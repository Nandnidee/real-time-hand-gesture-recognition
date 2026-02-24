import cv2
import numpy as np
import os
import mediapipe as mp
import time

# ================= CONFIG =================
DATA_PATH = "DATA"
action = "sign_name"
num_samples = 200
camera_index = 0
stable_seconds_required = 2
capture_delay = 0.08
# ==========================================

os.makedirs(os.path.join(DATA_PATH, action), exist_ok=True)

mp_hands = mp.solutions.hands
mp_draw = mp.solutions.drawing_utils

hands = mp_hands.Hands(
    static_image_mode=False,
    max_num_hands=2,
    min_detection_confidence=0.8,
    min_tracking_confidence=0.8
)

def extract_landmarks(results):
    left = np.zeros(63)
    right = np.zeros(63)

    if results.multi_hand_landmarks:
        for idx, hand_landmarks in enumerate(results.multi_hand_landmarks):

            coords = []
            for lm in hand_landmarks.landmark:
                coords.extend([lm.x, lm.y, lm.z])

            label = results.multi_handedness[idx].classification[0].label

            if label == "Left":
                left = np.array(coords)
            else:
                right = np.array(coords)

    return np.concatenate([left, right])  # always 126


cap = cv2.VideoCapture(camera_index)

if not cap.isOpened():
    print("Camera not opening")
    exit()

print("Hold hand steady to start capture")

stable_start = None
capturing = False
count = 0
last_capture_time = 0

while cap.isOpened():

    ret, frame = cap.read()
    if not ret:
        break

    image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(image)

    hand_detected = results.multi_hand_landmarks is not None

    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            mp_draw.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    # -------- Stability Check --------
    if hand_detected and not capturing:

        if stable_start is None:
            stable_start = time.time()

        elapsed = time.time() - stable_start

        cv2.putText(frame,
                    f"Hold steady: {int(elapsed)}/{stable_seconds_required}",
                    (10, 40),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.8, (0, 255, 0), 2)

        if elapsed >= stable_seconds_required:
            capturing = True
            print("Stable. Capturing started.")

    else:
        if not capturing:
            stable_start = None
            cv2.putText(frame,
                        "Show at least ONE hand",
                        (10, 40),
                        cv2.FONT_HERSHEY_SIMPLEX,
                        0.8, (0, 0, 255), 2)

    # -------- Capture --------
    if capturing and count < num_samples and hand_detected:

        current_time = time.time()

        if current_time - last_capture_time > capture_delay:

            landmarks = extract_landmarks(results)

            np.save(
                os.path.join(DATA_PATH, action, f"{count}.npy"),
                landmarks
            )

            count += 1
            last_capture_time = current_time

        cv2.putText(frame,
                    f"Capturing: {count}/{num_samples}",
                    (10, 80),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.8, (0, 0, 255), 2)

    if count >= num_samples:
        print("Capture Complete.")
        break

    cv2.imshow("Data Collection", frame)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
