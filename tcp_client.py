import socket
import json

def process_payment_with_sdk(amount, name, currency):
    host = '127.0.0.1'  # Localhost
    port = 5000         # Same port as the .NET server

    # Prepare the payload
    payload = json.dumps({
        "amount": amount,
        "name": name,
        "currency": currency
    })

    try:
        # Connect to the .NET server
        with socket.create_connection((host, port)) as client:
            # Send the payload
            utf8_payload = payload.encode('utf-8')
            client.sendall(utf8_payload)
            client.shutdown(socket.SHUT_WR)
            cnt = 1
            response = ""
            while True or cnt > 10:
                cnt += 1
                part = client.recv(1024)
                if not part:
                    break
                response += part.decode('utf-8').strip()

            print(f"Response from server: {response}")
            return json.loads(response)

    except Exception as e:
        return {"status": "error", "message": str(e)}

process_payment_with_sdk(20, 'sami', '$')