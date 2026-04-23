@echo off
chcp 65001 > nul
cd /d "%~dp0"

set "CONDA_HOOK=D:\Anaconda\shell\condabin\conda-hook.ps1"
if not exist "%CONDA_HOOK%" (
    set "CONDA_HOOK=C:\ProgramData\anaconda\shell\condabin\conda-hook.ps1"
)

echo 正在调用 Anaconda 启动环境...


echo [1/2] 正在启动 WebSocket 网关服务 (trans.py)...
start powershell -ExecutionPolicy ByPass -NoExit -Command "& '%CONDA_HOOK%'; conda activate GPS; cd '%~dp0'; python trans.py"
timeout /t 2 /nobreak > nul

echo [2/2] 正在启动前端 Web 服务...
:: 【关键修复】：去掉了路径里的 GIS_html，让服务器真正涵盖整个项目包含 assets
start powershell -WindowStyle Hidden -ExecutionPolicy ByPass -Command "& '%CONDA_HOOK%'; conda activate GPS; cd '%~dp0'; python -m http.server 8000"

timeout /t 2 /nobreak > nul

echo 🚀 Web 与网关服务已启动！正在打开浏览器...
:: 【修改】：simulator.py 被移除后，gm 测试面板的指令 C# 并未处理，改为直接打开玩家前端
start http://127.0.0.1:8000/GIS_html/index.html