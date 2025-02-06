# Funnel Project

## Overview

The Funnel project consists of a server and a CLI client that work together to funnel HTTP requests from the server URL to a local URL. The server proxies the requests to the CLI client, which processes the requests and returns the responses back to the server.

## Project Structure

- `Funnel.CliClient`: The CLI client that processes requests from the server.
- `Funnel.Server`: The server that proxies requests to the CLI client.

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio or JetBrains Rider

## Getting Started

### Running the Server

1. Navigate to the `Funnel.Server` directory.
2. Run the server using the following command:

   ```sh
   dotnet run
   ```

### Running the CLI Client

1. Navigate to the `Funnel.CliClient` directory.
2. Run the client using the following command:

   ```sh
   dotnet run -- https://localhost:7092 https://httpbin.org/
   ```

   The arguments are:
    - `server_url`: The server URL to connect to.
    - `local_url`: The local URL to funnel requests to.

## Configuration

The `launchSettings.json` file in the `Funnel.CliClient` project contains the default command line arguments and environment variables for development.

## Code Overview

### Funnel.CliClient

The `Program.cs` file sets up the CLI client, configures the HTTP client, and starts the application.

### Funnel.Server

The `Program.cs` file sets up the server, configures the gRPC services, and maps the request handling logic.

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Contact

For any questions or issues, please open an issue on GitHub.
```