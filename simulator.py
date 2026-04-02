import asyncio
import json
import logging

# 配置日志输出格式
logging.basicConfig(level=logging.INFO, format='%(asctime)s - [模拟主干] - %(message)s')

HOST = '127.0.0.1'
PORT = 9010

# 这是一个 1x1 像素的红色 PNG 图片的纯 Base64 编码
DUMMY_BASE64_IMAGE = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="

async def handle_intermediary(reader, writer):
    addr = writer.get_extra_info('peername')
    logging.info(f"中继脚本已成功连接: {addr}")

    # 任务 1：不断读取来自移动端的 GPS 数据
    async def read_loop():
        while True:
            try:
                # 读取以 \n 结尾的整行数据
                data = await reader.readline()
                if not data:
                    break
                logging.info(f"收到上行 GPS 数据: {data.decode('utf-8').strip()}")
            except Exception as e:
                logging.error(f"读取数据流异常: {e}")
                break

    # 任务 2：定时向移动端下发 JSON 指令与图片
    async def write_loop():
        counter = 1
        while True:
            try:
                await asyncio.sleep(1000) # 每 10 秒触发一次发送
                
                payload = {
                    "text": f"系统广播: 这是来自模拟服务器的测试消息 #{counter}",
                    "image": DUMMY_BASE64_IMAGE
                }
                
                # 序列化为 JSON -> 添加换行符 -> 强制 UTF-8 编码
                json_str = json.dumps(payload)
                msg_bytes = f"{json_str}\n".encode('utf-8')
                
                writer.write(msg_bytes)
                await writer.drain() # 确保数据推送到底层 socket
                
                logging.info(f"已下发包含图片的测试消息 #{counter}，大小: {len(msg_bytes)} 字节")
                counter += 1
            except Exception as e:
                logging.error(f"发送数据流异常: {e}")
                break

    # 并发执行读和写两个循环
    read_task = asyncio.create_task(read_loop())
    write_task = asyncio.create_task(write_loop())
    
    # 只要其中一个任务断开，就结束连接
    await asyncio.wait([read_task, write_task], return_when=asyncio.FIRST_COMPLETED)
    
    logging.info(f"中继脚本已断开连接: {addr}")
    writer.close()
    await writer.wait_closed()

async def main():
    server = await asyncio.start_server(handle_intermediary, HOST, PORT)
    addrs = ', '.join(str(sock.getsockname()) for sock in server.sockets)
    logging.info(f"✅ 模拟 C# 服务器启动成功，正在监听 {addrs}")

    async with server:
        await server.serve_forever()

if __name__ == '__main__':
    # 注意：如果你还在 Jupyter Notebook 里运行，请记得加上 nest_asyncio.apply()
    # 或者把下面几行改成 await main()
    import sys
    if 'ipykernel' in sys.modules:
        import nest_asyncio
        nest_asyncio.apply()
        
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        logging.info("模拟服务器已手动关闭")