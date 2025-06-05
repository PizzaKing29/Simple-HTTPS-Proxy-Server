# Simple HTTPS Proxy in C#

This is a minimal HTTPS proxy server written in C# using sockets and async/await. It supports basic `CONNECT` tunneling for HTTPS connections.

## 📌 Features

- Listens for incoming TCP connections on a specified IP and port.
- Parses `CONNECT` requests to establish an HTTPS tunnel.
- Resolves target domains via DNS.
- Relays encrypted traffic between client (browser) and remote server.
- Fully asynchronous, handles multiple connections in parallel.

## 🚀 Getting Started

### Requirements

- .NET 6.0 or later
- A browser or tool configured to use an HTTPS proxy (e.g., Firefox)

### Running the Proxy

1. Clone the repository or copy the code.
2. Open the project in your editor.
3. Modify the following variables in the `Main` method to change the proxy IP and port:

```csharp
string InternetProtocol = "127.0.0.1"; // Change this if needed
int Port = 8888;                      // Change the port number here