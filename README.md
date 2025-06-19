#  SecureFileStorage

A **proof-of-concept** secure file storage system implementing multi-layer encryption with a microservices architecture. Built as an academic project to demonstrate cryptographic principles, secure system design, and modern development practices.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=flat&logo=blazor)](https://blazor.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

>  **Academic Project Notice**: This is a proof-of-concept implementation created for educational purposes. It demonstrates cryptographic concepts but is **not production-ready** due to performance and security limitations outlined below.

##  Features

- **Multi-layer Encryption**: AES-256-CBC + XOR obfuscation
- **File Integrity**: SHA-512 hash verification
- **User Management**: JWT-based authentication and authorization
- **Microservices Architecture**: Modular, scalable design
- **Audit Logging**: Comprehensive file access tracking
- **Modern UI**: Blazor WebAssembly frontend
- **RESTful API**: Clean API design with proper error handling

##  Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Blazor Web    │    │   ASP.NET Core  │    │  PostgreSQL     │
│   Frontend      ├────┤   API Gateway   ├────┤  Database       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                       ┌────────┼────────┐
                       │        │        │
               ┌───────▼────┐ ┌─▼──────┐ ┌▼────────────┐
               │Encryption  │ │Storage │ │Key Mgmt     │
               │Service     │ │Service │ │Service      │
               └────────────┘ └────────┘ └─────────────┘
```

##  Technology Stack

### Backend
- **.NET 8** - Modern C# framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **PostgreSQL** - Primary database
- **Serilog** - Structured logging

### Security & Cryptography
- **System.Security.Cryptography** - AES encryption
- **libsodium-net** - Additional cryptographic functions
- **JWT Bearer Authentication** - Token-based auth
- **RSA-2048** - Key exchange and digital signatures

### Frontend
- **Blazor WebAssembly** - Modern web UI framework
- **Bootstrap 5** - Responsive design
- **JavaScript** - File handling utilities

##  Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 12+](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/SecureFileStorage.git
   cd SecureFileStorage
   ```

2. **Setup the database**
   ```bash
   # Update connection string in appsettings.json
   cd src/SecureFileStorage.API
   dotnet ef database update
   ```

3. **Configure application settings**
   ```json
   // src/SecureFileStorage.API/appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=SecureFileStorage;Username=postgres;Password=yourpassword"
     },
     "JwtSettings": {
       "SecretKey": "your-256-bit-secret",
       "Issuer": "SecureFileStorage",
       "Audience": "SecureFileStorage"
     }
   }
   ```

4. **Run the application**
   ```bash
   # Terminal 1 - API
   cd src/SecureFileStorage.API
   dotnet run

   # Terminal 2 - Web UI
   cd src/SecureFileStorage.Web
   dotnet run
   ```

5. **Access the application**
   - API: `https://localhost:7001`
   - Web UI: `https://localhost:7002`
   - Swagger: `https://localhost:7001/swagger`

##  Encryption Process

### File Upload Flow
```csharp
1. Generate unique AES-256 key (32 bytes)
2. Generate unique XOR key (32 bytes)
3. Encrypt file with AES-256-CBC + random IV
4. Apply XOR obfuscation layer
5. Compute SHA-512 integrity hash
6. Encrypt keys with user's RSA public key
7. Store encrypted data + metadata
```

### Security Layers
- **Layer 1**: AES-256-CBC encryption with random IV
- **Layer 2**: XOR obfuscation for additional data protection
- **Integrity**: SHA-512 hash verification
- **Authentication**: JWT-based user authentication
- **Authorization**: Role-based access control

## Current Limitations

>  **Important**: This project has several limitations that make it unsuitable for production use:

### Performance Issues
- **Memory Usage**: Entire files loaded into memory (limits file size to available RAM)
- **Scalability**: No streaming encryption - poor performance with large files
- **Concurrency**: Limited concurrent user support

### Security Concerns
- **XOR Vulnerability**: XOR with key reuse creates cryptographic weaknesses
- **Server-side Key Management**: Not zero-knowledge - server handles key encryption
- **No Authenticated Encryption**: AES-CBC without proper authentication modes

### Infrastructure Limitations
- **Local Storage**: Files stored on local filesystem
- **No Load Balancing**: Single instance deployment
- **Limited Error Recovery**: Basic error handling

##  Planned Improvements

### High Priority
- [ ] **Streaming Encryption** - Process files in chunks to handle large files
- [ ] **Replace XOR with ChaCha20-Poly1305** - Proper AEAD encryption
- [ ] **Client-side Key Derivation** - True zero-knowledge architecture
- [ ] **AES-GCM Implementation** - Authenticated encryption

### Medium Priority
- [ ] **Cloud Storage Integration** - AWS S3, Azure Blob support
- [ ] **Redis Caching** - Performance optimization
- [ ] **Docker Containerization** - Easy deployment
- [ ] **Comprehensive Testing** - Unit, integration, and security tests

### Future Enhancements
- [ ] **Multi-factor Authentication** - Enhanced security
- [ ] **File Sharing** - Secure file sharing between users
- [ ] **Audit Dashboard** - Advanced monitoring and reporting
- [ ] **API Rate Limiting** - DoS protection

##  Testing

```bash
# Run unit tests
dotnet test src/SecureFileStorage.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

##  Learning Objectives

This project demonstrates understanding of:

- **Cryptographic Principles**: Symmetric/asymmetric encryption, hashing
- **Secure System Design**: Defense in depth, principle of least privilege
- **Modern Architecture**: Microservices, API-first design
- **Full-stack Development**: Backend APIs, frontend UI, database design
- **.NET Ecosystem**: Entity Framework, dependency injection, logging

##  Contributing

This is an academic project, but feedback and suggestions are welcome:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/improvement`)
3. Commit your changes (`git commit -am 'Add improvement'`)
4. Push to the branch (`git push origin feature/improvement`)
5. Open a Pull Request


##  Academic Context

**Course**: Secure and Robust Programming  
**Level**: 4th Year Computer Science  
**Focus**: Applied cryptography, secure system design, modern development practices

---
