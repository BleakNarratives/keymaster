import pytest
from fastapi.testclient import TestClient
from src.keymaster_api import app

client = TestClient(app)

def test_calibrate_endpoint():
    payload = {
        "arw_points": [{"x": 1, "y": 2, "z": 3}],
        "rwp_points": [{"x": 4, "y": 5, "z": 6}]
    }
    response = client.post("/api/calibrate", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert "rmse" in data
    assert "transform_elements" in data
    assert len(data["transform_elements"]) == 16
    assert "source_platform" in data
    assert "num_points_used" in data

def test_analyze_endpoint():
    calibration_result = {
        "rmse": 0.0012,
        "transform_elements": [1.0 if i % 5 == 0 else 0.0 for i in range(16)],
        "source_platform": "python",
        "num_points_used": 1
    }
    payload = {
        "result": calibration_result,
        "prompt": "Analyze this result."
    }
    response = client.post("/api/analyze", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert "analysis" in data
    assert "RMSE" in data["analysis"]
