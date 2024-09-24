import time
import socket

def start_udp():
    # Create a UDP socket
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server_address = ('localhost', 52020)
    print("Server is listening on port 52020...")
    try:
        while True:
            # Send 3 bytes to the client
            client_socket.sendto(b'\xff'*5, server_address)
            print("Sent 5 bytes to the client")
            
            # Receive data from the client
            data, _ = client_socket.recvfrom(5)
            print(f"Received {data}")

            time.sleep(0.1)
    finally:
        client_socket.close()

if __name__ == "__main__":
    start_udp()