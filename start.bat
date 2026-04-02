@echo off
:: 声明使用 UTF-8 编码，防止中文路径或提示乱码
chcp 65001 > nul

:: 自动获取并切换到当前 bat 文件所在的绝对目录
cd /d "%~dp0"

:: 根据你的快捷方式路径，自动定位 Anaconda 的 PowerShell 初始化脚本
set "CONDA_HOOK=D:\Anaconda\shell\condabin\conda-hook.ps1"

:: 容错处理：如果上面那个路径不存在，尝试另一个常见路径
if not exist "%CONDA_HOOK%" (
    set "CONDA_HOOK=C:\ProgramData\anaconda\shell\condabin\conda-hook.ps1"
)

echo 正在调用 Anaconda PowerShell 启动环境...

echo [1/3] 正在启动主服务器模拟器 (Conda: GPS)...
start powershell -ExecutionPolicy ByPass -NoExit -Command "& '%CONDA_HOOK%'; conda activate GPS; cd '%~dp0'; python simulator.py"
timeout /t 2 /nobreak > nul

echo [2/3] 正在启动中间件通信脚本 (Conda: GPS)...
start powershell -ExecutionPolicy ByPass -NoExit -Command "& '%CONDA_HOOK%'; conda activate GPS; cd '%~dp0'; python trans.py"
timeout /t 2 /nobreak > nul

echo [3/3] 正在启动前端 Web 服务...
start powershell -ExecutionPolicy ByPass -NoExit -Command "& '%CONDA_HOOK%'; conda activate GPS; cd '%~dp0GIS_html'; python -m http.server 8000"

:: 给 Web 服务一点启动时间，然后打开浏览器
timeout /t 1 /nobreak > nul
start http://127.0.0.1:8000