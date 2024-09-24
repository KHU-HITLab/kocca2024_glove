import serial
import serial.tools.list_ports
import socket
import sys

UDP_PORT = None
TARGET_VID = 0x2e8a
TARGET_PID = 0xf00a

def main():
    if len(sys.argv) != 2:
        print('provide udp port number as an argument')
        sys.exit(1)
    global UDP_PORT
    UDP_PORT = int(sys.argv[1])

    try:
        serial_port = open_serial()
        server_socket = start_udp_server()
        values = bytearray(5)
        while True:
            # Receive commands from the client
            try:
                data, client_address = server_socket.recvfrom(5)
                print(f"Received {data} from {client_address}")
                serial_port.write(data)
            except socket.timeout:
                print('Timeout occurred')
                serial_port.write(b'\x00\x00\x00\x00\x00')
                continue
            # Simulate a simple reference follower system
            for i in range(5):
                error = data[i] - values[i]
                if abs(error) > 10:
                    values[i] += int(error * 0.1)
                else:
                    values[i] += error
            # Send values to the client
            server_socket.sendto(values, client_address)
            print("Sent 5 bytes to the client")
    except Exception as e:
        print(e)
    finally:
        if serial_port:
            serial_port.close()
        if server_socket:
            server_socket.close()
        print('Closed connections')
        sys.exit(1)

def open_serial():
    com_ports = serial.tools.list_ports.comports()
    for port in com_ports:
        name = port.device
        vid = port.vid
        pid = port.pid
        # list all available serial ports
        if vid and pid:
            print(f'{name} {vid:X}:{pid:X}')
        else:
            print(f'{name} no vid/pid')
        # find raspberry pi pico w
        if vid == TARGET_VID and pid == TARGET_PID:
            print('target serial port found')
            ser = serial.Serial(name, baudrate=921600)
            if ser.is_open:
                print('serial port is open')
                break
            else:
                raise Exception('failed to open serial port')
    return ser

def start_udp_server():
    # create a udp socket
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.settimeout(1)
    s.bind(('127.0.0.1', UDP_PORT))
    print('udp socket created')
    return s

if __name__ == '__main__':
    main()