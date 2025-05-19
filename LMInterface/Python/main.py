from fastapi import FastAPI
from pydantic import BaseModel
from fastapi.encoders import jsonable_encoder
import sys
import io

class ScriptRequest(BaseModel):
    script: str



app = FastAPI()

@app.get("/")
async def root():
    return {"message": "Server active"}

@app.post("/exec-script")
def exec_script(req: ScriptRequest):

    # Save the original stdout and redirect stdout to the buffer
    old_stdout = sys.stdout
    buffer = io.StringIO()
    sys.stdout = buffer

    try:
        exec(req.script, {})
        # Get all print()'s
        output = buffer.getvalue()

        if output is None:
            safe_result = "Nothing has been printed!"

        return {"output": output}
    except Exception as e:
        return {"error": str(e)}
    finally:
        # Restore original
        sys.stdout = old_stdout  