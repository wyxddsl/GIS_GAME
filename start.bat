@echo off
chcp 65001 > nul
cd /d "%~dp0"

set "CONDA_HOOK=D:\Anaconda\shell\condabin\conda-hook.ps1"
if not exist "%CONDA_HOOK%" (
    set "CONDA_HOOK=C:\ProgramData\anaconda\shell\condabin\conda-hook.ps1"
)

echo 正在调用 Anaconda 启动环境...

echo [1/3] 正在启动模拟服务器 (simulator.py)...
start powershell -ExecutionPolicy ByPass -NoExit -Command "& '%CONDA_HOOK%'; conda activate GPS; cd '%~dp0'; python simulator.py"
timeout /t 2 /nobreak > nul

echo [2/3] 正在启动 WebSocket 中继通信网关 (trans.py)...
start powershell -ExecutionPolicy ByPass -NoExit -Command "& '%CONDA_HOOK%'; conda activate GPS; cd '%~dp0'; python trans.py"
timeout /t 2 /nobreak > nul

echo [3/3] 正在启动前端 Web 服务...
:: 【关键修复】：去掉了路径里的 GIS_html，让服务器真正涵盖整个项目包含 assets
start powershell -WindowStyle Hidden -ExecutionPolicy ByPass -Command "& '%CONDA_HOOK%'; conda activate GPS; cd '%~dp0'; python -m http.server 8000"

timeout /t 2 /nobreak > nul

echo 🚀 所有服务已启动！正在打开浏览器...
:: 【关键修复】：由于服务器起点变高了，访问网页时需要带上 /GIS_html/
start http://127.0.0.1:8000/GIS_html/gm.html