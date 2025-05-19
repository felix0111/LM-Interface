# LMInterface
A WinUI 3 application used to communicate with LLMs via API requests e.g. LM Studio.  
(Primarily tested with Qwen3-30B-A3B through LM Studio, models with notably different chat templates might be buggy)

![Screenshot 2025-05-19 191149](https://github.com/user-attachments/assets/c360ada3-3ab7-422f-b9a0-61617a2b1352)

## Current features:
- Multiple conversations
- Automatically prepend date to messages
- Editing and removing messages
- Custom system message
- Switch between Thinking and No-Thinking mode

## Tools the model can use:
- WebTool
  - access to website content
  - optionally filters the fetched html content using xpath
- PythonTool
  - execution of python scripts and fetching of output logs
  - python + numpy, uvicorn, fastapi must be preinstalled!
  - NOTE: the uvicorn server might not get terminated when the app is forcefully closed
  - WARNING: use at your own risk as the model basically gains access to your environment,  
    in the future i'll try to find a way to run the scripts in an isolated environment
