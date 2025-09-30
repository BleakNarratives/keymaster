# KeyMaster API Documentation

## Endpoints

### POST /api/calibrate
- **Description:** Receives raw calibration points and returns a computed transformation (mock result).
- **Request Body:**
  - `arw_points`: List of 3D points (x, y, z)
  - `rwp_points`: List of 3D points (x, y, z)
- **Response:**
  - `rmse`: float
  - `transform_elements`: list of 16 floats
  - `source_platform`: str
  - `num_points_used`: int

### POST /api/analyze
- **Description:** Receives a CalibrationResult and an analysis prompt for the AI agent.
- **Request Body:**
  - `result`: CalibrationResultModel
  - `prompt`: str
- **Response:**
  - `analysis`: str

## Models

### PointModel
- `x`: float
- `y`: float
- `z`: float

### CalibrationResultModel
- `rmse`: float
- `transform_elements`: list of 16 floats
- `source_platform`: str
- `num_points_used`: int

### CalibrationRequestModel
- `arw_points`: list of PointModel
- `rwp_points`: list of PointModel

### AnalysisRequestModel
- `result`: CalibrationResultModel
- `prompt`: str

---

For more details, see the OpenAPI docs at `/docs` when running the backend server.