# FHIR to HL7 Conversion System

A microservices-based system that converts FHIR R4 STU3 objects to HL7 v2.4 messages and forwards them to the Ontario PCR endpoint.

## Features

- FHIR to HL7 v2.4 conversion (ADT messages: A28, A31, A40)
- Message transmission to Ontario PCR
- Message logging and tracking
- User interface for message management and resending
- Clinic-level controls
- Daily digest notifications for failures

## Architecture

The system is composed of several microservices:

1. **FHIR Receiver Service**: Entry point for FHIR resources
2. **Conversion Service**: Transforms FHIR to HL7
3. **Transmission Service**: Sends HL7 messages to Ontario PCR
4. **Logging Service**: Records message metadata and status
5. **Notification Service**: Sends daily digest emails
6. **Resend API Service**: Provides REST endpoints for the UI
7. **UI Application**: React-based frontend for message management

## Prerequisites

- .NET 8.0 SDK
- Node.js (LTS version)
- SQL Server (or SQL Server container)
- Docker and Docker Compose
- Azure Service Bus (or emulator)

## Development Setup

### 1. Clone the repository

```bash
git clone https://github.com/yourusername/fhir-to-hl7-converter.git
cd fhir-to-hl7-converter
```

### 2. Start the infrastructure services

```bash
docker-compose up -d sqlserver servicebus-emulator
```

### 3. Initialize the database

```bash
cd backend/LoggingService
dotnet ef database update
```

### 4. Start the backend services

Open multiple terminal windows, one for each service:

```bash
# FhirReceiverService
cd backend/FhirReceiverService
dotnet run

# ConversionService
cd backend/ConversionService
dotnet run

# LoggingService
cd backend/LoggingService
dotnet run

# TransmissionService
cd backend/TransmissionService
dotnet run

# NotificationService
cd backend/NotificationService
dotnet run

# ResendApiService
cd backend/ResendApiService
dotnet run
```

### 5. Start the frontend

```bash
cd frontend
npm install
npm start
```

The UI will be available at: http://localhost:3000

## Configuration

Each service has its own `appsettings.json` file for configuration. Key settings:

### Service Bus Configuration

```json
"ServiceBus": {
  "ConnectionString": "Endpoint=sb://localhost:9898/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=yourkey",
  "FhirQueueName": "fhir-queue",
  "Hl7QueueName": "hl7-queue",
  "StatusQueueName": "status-queue",
  "RetryQueueName": "retry-queue"
}
```

### Database Connection

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FhirToHl7;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=true"
}
```

### Ontario PCR Endpoint

```json
"Transmission": {
  "EndpointUrl": "https://ontariopcr.example.com/api/hl7",
  "MaxRetryAttempts": 3,
  "RetryDelayMilliseconds": 5000,
  "UseBackoffStrategy": true
}
```

### Email Configuration

```json
"EmailOptions": {
  "SmtpServer": "smtp.outlook.com",
  "SmtpPort": 587,
  "Username": "youremail@example.com",
  "Password": "yourpassword",
  "SenderEmail": "notifications@example.com",
  "SenderName": "Ontario PCR Integration",
  "EnableSsl": true
}
```

## Project Structure

```
├── backend/
│   ├── Common/                    # Shared models and services
│   ├── FhirReceiverService/       # Entry point for FHIR resources
│   ├── ConversionService/         # Transforms FHIR to HL7
│   ├── TransmissionService/       # Sends HL7 messages to Ontario PCR
│   ├── LoggingService/            # Records message metadata and status
│   ├── NotificationService/       # Sends daily digest emails
│   └── ResendApiService/          # REST API for the UI
│
├── frontend/                      # React-based UI
│   ├── public/
│   └── src/
│       ├── components/            # Reusable UI components
│       ├── context/               # React context for state management
│       ├── pages/                 # Page components
│       └── services/              # API clients
│
├── k8s/                           # Kubernetes deployment files
│
└── docker-compose.yml             # Development environment setup
```

## API Endpoints

### FHIR Receiver Service

- `POST /api/fhir` - Receive FHIR resource
- `GET /api/fhir/status` - Check service status

### Conversion Service

- `POST /api/conversion/a28` - Convert to A28 message
- `POST /api/conversion/a31` - Convert to A31 message
- `POST /api/conversion/a40` - Convert to A40 message

### Logging Service

- `POST /api/logging` - Log a message
- `GET /api/logging` - Get messages with filters
- `GET /api/logging/{id}` - Get a specific message
- `PUT /api/logging/{id}/status` - Update message status

### Resend API Service

- `GET /api/messages` - List messages with filters
- `GET /api/messages/{id}` - Get a specific message
- `POST /api/messages/{id}/resend` - Resend a message
- `POST /api/messages/batch-resend` - Resend multiple messages
- `PUT /api/messages/{id}/content` - Update message content
- `GET /api/clinics` - List clinics
- `GET /api/clinics/{id}` - Get a specific clinic
- `PUT /api/clinics/{id}/status` - Update clinic status

## Sample FHIR Patient Resource

Here's a sample FHIR Patient resource you can use to test the system:

```json
{
  "resourceType": "Patient",
  "id": "example",
  "meta": {
    "tag": [
      {
        "system": "http://terminology.hl7.org/CodeSystem/v2-0203",
        "code": "A28"
      }
    ]
  },
  "text": {
    "status": "generated",
    "div": "<div>John Smith</div>"
  },
  "identifier": [
    {
      "system": "http://example.org/fhir/identifier/mrn",
      "value": "12345"
    }
  ],
  "name": [
    {
      "family": "Smith",
      "given": [
        "John"
      ]
    }
  ],
  "gender": "male",
  "birthDate": "1974-12-25",
  "address": [
    {
      "line": [
        "123 Main St"
      ],
      "city": "Anytown",
      "state": "ON",
      "postalCode": "A1B 2C3"
    }
  ],
  "telecom": [
    {
      "system": "phone",
      "value": "555-555-5555",
      "use": "home"
    }
  ],
  "managingOrganization": {
    "reference": "Organization/CLINIC001"
  }
}
```

## Deployment

### Kubernetes Deployment

The project includes Kubernetes deployment files in the `k8s/` directory. To deploy to a Kubernetes cluster:

```bash
# Update image repository in k8s files
# Replace ${ACR_REGISTRY} with your actual registry

# Deploy services
kubectl apply -f k8s/
```

### Azure Deployment

For production deployment to Azure:

1. Create an Azure SQL Database
2. Set up Azure Service Bus namespace and queues
3. Deploy microservices to AKS (Azure Kubernetes Service)
4. Configure environment variables in Azure DevOps pipelines

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit your changes: `git commit -am 'Add new feature'`
4. Push to the branch: `git push origin feature/my-feature`
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
