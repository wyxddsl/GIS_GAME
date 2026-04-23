import asyncio
import websockets
import json
import logging
import sys
import io
# Python 控制台输出 UTF-8，防止控制台打印出乱码
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
# 配置日志输出格式
logging.basicConfig(
    level=logging.INFO, 
    format='%(asctime)s - [网关] - %(levelname)s - %(message)s',
    handlers=[logging.StreamHandler(sys.stdout)]
)

# ================= 配置区 =================
# C# 核心服务器配置 (TCP)
C_SHARP_HOST = '127.0.0.1' 
C_SHARP_PORT = 9010        

# 手机端/前端连接配置 (WebSocket)
WS_HOST = '0.0.0.0'
WS_PORT = 8765
# ==========================================

# 全局状态
connected_ws_clients = set()
tcp_writer = None

async def tcp_client_loop():
    """维护与 C# 服务器的 TCP 长连接，并接收来自 C# 的下发指令"""
    global tcp_writer
    
    while True:
        try:
            logging.info(f"🔄 正在尝试连接 C# 核心服务器 ({C_SHARP_HOST}:{C_SHARP_PORT})...")
            reader, writer = await asyncio.open_connection(C_SHARP_HOST, C_SHARP_PORT)
            tcp_writer = writer
            logging.info("✅ 已成功连接到 C# 核心服务器！")

            # 持续读取 C# 发来的数据（按行读取 \n）
            while True:
                data = await reader.readline()
                if not data:
                    logging.warning("⚠️ C# 服务器主动断开连接。")
                    break
                
                payload = data.decode('utf-8').strip()
                if payload:
                    try:
                        # 尝试解析 JSON，用于优化日志输出（避免高频心跳刷屏）
                        msg_dict = json.loads(payload)
                        msg_type = msg_dict.get("msg_type") or msg_dict.get("type")
                        
                        # 广播给所有已连接的手机客户端
                        if connected_ws_clients:
                            websockets.broadcast(connected_ws_clients, payload)
                            # 过滤掉高频的无关紧要日志，保持控制台清爽
                            if msg_type not in ["C2S_POS", "TEXT_MSG"]: 
                                logging.info(f"📤 [C# -> 手机] 广播业务消息: {msg_type}")
                        else:
                            logging.debug(f"丢弃消息（无手机端连接）: {msg_type}")

                    except json.JSONDecodeError:
                        logging.error(f"❌ 收到来自 C# 的非预期数据格式: {payload}")

        except ConnectionRefusedError:
            logging.error("❌ 无法连接到 C# 服务器，请确认 FormMain 中的 TCP 监听已启动。")
        except Exception as e:
            logging.error(f"❌ TCP 连接异常: {e}")
        finally:
            tcp_writer = None
            logging.info("⏳ 5秒后尝试重新连接 C# 服务器...\n")
            await asyncio.sleep(5)

async def ws_handler(websocket):
    """处理来自手机端/前端的 WebSocket 连接，并将上行数据转发给 C#"""
    global tcp_writer
    client_address = websocket.remote_address
    connected_ws_clients.add(websocket)
    logging.info(f"📱 手机端已连接: {client_address} (当前在线人数: {len(connected_ws_clients)})")

    try:
        async for message in websocket:
            # 接收到手机端发来的 JSON 字符串
            if tcp_writer:
                try:
                    # 仅作日志过滤提取
                    msg_dict = json.loads(message)
                    msg_type = msg_dict.get("msg_type") or msg_dict.get("type")
                    
                    if msg_type != "C2S_POS":
                        logging.info(f"📥 [手机 -> C#] 转发上行消息: {msg_type}")
                except json.JSONDecodeError:
                    pass

                # 核心转发逻辑：为 C# 的 StreamReader.ReadLine() 追加换行符
                tcp_writer.write(f"{message}\n".encode('utf-8'))
                await tcp_writer.drain()
            else:
                logging.debug("⚠️ C# 服务器未连接，丢弃手机端上报数据。")
                
    except websockets.exceptions.ConnectionClosed as e:
        pass # 正常断开不需要打印错误堆栈
    finally:
        connected_ws_clients.remove(websocket)
        logging.info(f"📵 手机端已断开: {client_address} (当前在线人数: {len(connected_ws_clients)})")

async def main():
    # 启动 TCP 客户端后台任务（连接 C#）
    asyncio.create_task(tcp_client_loop())
    
    # 启动 WebSocket 服务端（等待手机端连接）
    logging.info(f"🌐 正在启动 WebSocket 网关服务 ws://{WS_HOST}:{WS_PORT}")
    async with websockets.serve(ws_handler, WS_HOST, WS_PORT):
        await asyncio.Future()  # 永久运行

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        logging.info("🛑 网关服务已手动关闭。")