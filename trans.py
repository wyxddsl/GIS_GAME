import asyncio
import websockets
import json
import logging

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

# Configuration
C_SHARP_HOST = '127.0.0.1' # Replace with your C# server IP
C_SHARP_PORT = 9010        # Replace with your C# server Port
WS_HOST = '0.0.0.0'
WS_PORT = 8765

# Global state
connected_ws_clients = set()
tcp_writer = None

async def tcp_client_loop():
    """Maintains a persistent connection to the C# server and listens for messages."""
    global tcp_writer
    
    while True:
        try:
            logging.info(f"Attempting to connect to C# server at {C_SHARP_HOST}:{C_SHARP_PORT}...")
            reader, writer = await asyncio.open_connection(C_SHARP_HOST, C_SHARP_PORT)
            tcp_writer = writer
            logging.info("Connected to C# server successfully.")

            # Read loop: Expecting newline-terminated JSON strings from C#
            while True:
                data = await reader.readline()
                if not data:
                    logging.warning("C# server disconnected.")
                    break
                
                payload = data.decode('utf-8').strip()
                if payload and connected_ws_clients:
                    # Broadcast exactly as received to all mobile clients
                    websockets.broadcast(connected_ws_clients, payload)
                    logging.info(f"Broadcasted payload to {len(connected_ws_clients)} clients.")

        except Exception as e:
            logging.error(f"TCP Connection error: {e}")
        finally:
            tcp_writer = None
            logging.info("Reconnecting to C# server in 5 seconds...")
            await asyncio.sleep(5)

async def ws_handler(websocket):
    """Handles individual WebSocket connections from mobile clients."""
    global tcp_writer
    client_address = websocket.remote_address
    connected_ws_clients.add(websocket)
    logging.info(f"Mobile client connected: {client_address}. Total clients: {len(connected_ws_clients)}")

    try:
        async for message in websocket:
            # message is expected to be a JSON string containing GPS data
            if tcp_writer:
                # Forward to C#, adding a newline terminator for the C# stream reader
                tcp_writer.write(f"{message}\n".encode('utf-8'))
                await tcp_writer.drain()
            else:
                logging.debug("Dropped GPS data: No connection to C# server.")
                
    except websockets.exceptions.ConnectionClosed as e:
        logging.info(f"Mobile client disconnected: {client_address}")
    finally:
        connected_ws_clients.remove(websocket)

async def main():
    # Start the TCP client background task
    asyncio.create_task(tcp_client_loop())
    
    # Start the WebSocket server
    logging.info(f"Starting WebSocket server on ws://{WS_HOST}:{WS_PORT}")
    async with websockets.serve(ws_handler, WS_HOST, WS_PORT):
        await asyncio.Future()  # Run forever

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        logging.info("Server shutting down.")