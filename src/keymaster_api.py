from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
import logging
from pydantic import BaseModel, Field
from typing import List


# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s %(levelname)s %(message)s')
logger = logging.getLogger("keymaster_api")

from auth import router as auth_router

app = FastAPI()
app.include_router(auth_router)

class PointModel(BaseModel):
    x: float
    y: float
    z: float

class CalibrationResultModel(BaseModel):
    rmse: float
    transform_elements: List[float] = Field(..., min_items=16, max_items=16)
    source_platform: str
    num_points_used: int

class CalibrationRequestModel(BaseModel):
    arw_points: List[PointModel]
    rwp_points: List[PointModel]

class AnalysisRequestModel(BaseModel):
    result: CalibrationResultModel
    prompt: str

@app.post("/api/calibrate")
def calibrate(request: CalibrationRequestModel):
    try:
        logger.info(f"Received calibration request with {len(request.arw_points)} ARW points and {len(request.rwp_points)} RWP points.")
        mock_result = CalibrationResultModel(
            rmse=0.0012,
            transform_elements=[1.0 if i % 5 == 0 else 0.0 for i in range(16)],  # 4x4 identity matrix
            source_platform="python",
            num_points_used=len(request.arw_points)
        )
        logger.info(f"Returning mock calibration result: RMSE={mock_result.rmse}")
        return mock_result
    except Exception as e:
        logger.error(f"Error in /api/calibrate: {e}")
        return JSONResponse(status_code=500, content={"error": str(e)})

@app.post("/api/analyze")
def analyze(request: AnalysisRequestModel):
    try:
        logger.info(f"Received analysis request with RMSE={request.result.rmse} and prompt='{request.prompt}'")
        analysis_text = f"AI Analysis: The received RMSE is {request.result.rmse:.4f}. Data parsed successfully."
        logger.info("Returning mock analysis text.")
        return {"analysis": analysis_text}
    except Exception as e:
        logger.error(f"Error in /api/analyze: {e}")
        return JSONResponse(status_code=500, content={"error": str(e)})

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
