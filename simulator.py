import asyncio
import json
import logging

logging.basicConfig(level=logging.INFO, format='%(asctime)s - [C# 模拟服务器] - %(message)s')

HOST = '127.0.0.1'
PORT = 9010

# 模拟数据库状态
players_db = {}

async def test_flow_sequence(writer):
    """自动化测试流程，依次下发各类事件"""
    logging.info("🚀 开始执行 GM 测试流程...")
    
    # 1. 区域状态推送
    await send_json(writer, {
        "msg_type": "S2C_AREA",
        "data": { "area_id": 1, "area_name": "同济大学图书馆", "occupy_status": 0, "quiz_ids": "101", "tr_point_ids": "201" }
    })
    await asyncio.sleep(2)

    # 2. 宝藏推送
    await send_json(writer, {
        "msg_type": "S2C_TREASURE",
        "data": { "tr_id": 505, "content": "获得露天宝藏：神秘的测绘仪器", "score": 20, "ability": 0 }
    })
    await asyncio.sleep(2)

    # 3. 成就推送
    await send_json(writer, {
        "msg_type": "S2C_ACH",
        "data": { "name": "初出茅庐", "desc": "完成第一次坐标打卡", "img": "ach01.png" }
    })
    await asyncio.sleep(2)

    # 4. 题目发送
    await send_json(writer, {
        "type": "QUIZ_DATA",
        "data": {
            "total": 1, "this_num": 1, "qid": 101, "mode": "single_choice",
            "content": "以下哪种数据结构通常用于表示连续的地表高程？",
            "option_count": 4,
            "options": ["矢量多边形", "栅格(Raster/DEM)", "拓扑网络", "点云"],
            "answer_key": "1"
        }
    })

async def send_json(writer, payload_dict):
    """序列化并发送 JSON"""
    json_str = json.dumps(payload_dict, ensure_ascii=False)
    msg_bytes = f"{json_str}\n".encode('utf-8')
    writer.write(msg_bytes)
    await writer.drain()
    logging.info(f"📤 广播消息: {payload_dict.get('msg_type') or payload_dict.get('type')}")

async def handle_intermediary(reader, writer):
    addr = writer.get_extra_info('peername')
    logging.info(f"✅ 中继脚本(trans.py)已连接: {addr}")

    while True:
        try:
            data = await reader.readline()
            if not data: break
            
            payload = data.decode('utf-8').strip()
            msg = json.loads(payload)
            msg_type = msg.get('msg_type')
            uid = msg.get('uid', 'Unknown')

            # 处理来自中继的客户端消息
            if msg_type == 'C2S_POS':
                # 记录位置但不刷屏日志
                players_db[uid] = msg.get('data', {})
                
            elif msg_type == 'C2S_QUIZ_RESULT':
                logging.info(f"📝 收到玩家 [{uid}] 的答题结果: {msg['data']}")
                # 模拟加分并推送排行榜
                await send_json(writer, {"msg_type": "TEXT_MSG", "text": f"玩家 {uid} 答题成功！积分 +50"})
                await asyncio.sleep(1)
                await send_json(writer, {
                    "type": "RANKING_LIST",
                    "data": [
                        {"rank": 1, "uid": uid, "score": 2550},
                        {"rank": 2, "uid": "User_B", "score": 2100}
                    ]
                })

            elif msg_type == 'GM_CMD':
                command = msg.get('command')
                if command == 'START_TEST':
                    asyncio.create_task(test_flow_sequence(writer))
                elif command == 'STOP_GAME':
                    await send_json(writer, {"msg_type": "TEXT_MSG", "text": "⚠️ 游戏已由 GM 终止！"})

        except json.JSONDecodeError:
            logging.warning("收到非 JSON 格式数据")
        except Exception as e:
            logging.error(f"连接异常: {e}")
            break

    logging.info("❌ 中继脚本已断开")
    writer.close()
    await writer.wait_closed()

async def main():
    server = await asyncio.start_server(handle_intermediary, HOST, PORT)
    logging.info(f"🟢 C# 核心逻辑模拟器启动，监听 {PORT} 端口")
    async with server:
        await server.serve_forever()

if __name__ == '__main__':
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        logging.info("服务器关闭")