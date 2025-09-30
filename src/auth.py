from fastapi import APIRouter, HTTPException, status, Depends
from fastapi.security import HTTPBasic, HTTPBasicCredentials
import secrets

router = APIRouter()
security = HTTPBasic()

# Simple in-memory user store (for demo purposes)
users = {
    "admin": "password123"
}

def authenticate(credentials: HTTPBasicCredentials = Depends(security)):
    correct_username = secrets.compare_digest(credentials.username, "admin")
    correct_password = secrets.compare_digest(credentials.password, users["admin"])
    if not (correct_username and correct_password):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect username or password",
            headers={"WWW-Authenticate": "Basic"},
        )
    return credentials.username

@router.get("/api/protected")
def protected_route(username: str = Depends(authenticate)):
    return {"message": f"Hello, {username}. You are authenticated."}
