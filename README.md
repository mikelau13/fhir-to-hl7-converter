# FHIR to HL7 Conversion System

A microservices-based system that converts FHIR R4 STU3 objects to HL7 v2.4 messages and forwards them to the Ontario PCR endpoint.


## Documentations

- [Master Plan](/docs/masterplan.md)


## Features
- FHIR to HL7 v2.4 conversion (ADT messages: A28, A31, A40)
- Message transmission to Ontario PCR
- Message logging and tracking
- User interface for message management and resending
- Clinic-level controls
- Daily digest notifications for failures

## Technology Stack
- Backend: .NET Core 8.0
- Frontend: React.js with TypeScript
- Database: Azure SQL Database
- Message Queue: Azure Service Bus
- Containerization: Docker & Kubernetes

## Setup Instructions

To get the system up and running:

1. Install Entity Framework Core tools if it is not already installed
```bash
dotnet tool install --global dotnet-ef
dotnet ef --version
```

2. Setup the Database
```bash
cd backend/LoggingService
dotnet ef migrations add InitialCreate
dotnet ef database update
```

3. Start the Backend Services:
```bash
# Run each service in a separate terminal
cd backend/FhirReceiverService
dotnet run

cd backend/ConversionService
dotnet run

# And so on for each service
```

4. Start the Frontend:
```bash
cd frontend
npm install
npm start
```



```
PS fhir-to-hl7-converter-frontend\frontend> nvm install lts
PS fhir-to-hl7-converter-frontend\frontend> nvm use lts
PS fhir-to-hl7-converter-frontend\frontend> Set-ExecutionPolicy Unrestricted
PS fhir-to-hl7-converter-frontend\frontend> npm install
PS \> npx create-react-app fhir-to-hl7-converter-frontend --template typescript
PS fhir-to-hl7-converter-frontend\frontend> npm install @material-ui/core @material-ui/icons react-router-dom axios formik yup
PS fhir-to-hl7-converter-frontend\frontend> npm run start

PS \backend> dotnet restore
PS \backend> dotnet build
PS \backend> dotnet run
```



```
PS \frontend> nvm install lts
PS \frontend> nvm use lts
PS \frontend> Set-ExecutionPolicy Unrestricted
PS \frontend> npm install -g yarn
PS \frontend> yarn install
PS \frontend> yarn start
```

## Development
### Establish Development Workflow

1. Create development branches:
```bash
git checkout -b feature/conversion-service
git checkout -b feature/logging-service
git checkout -b feature/resend-api
```

2. Set up automated testing:
- Unit tests for each service
- Integration tests for API endpoints
- Frontend component tests


3. Configure CI/CD pipeline:
- Set up Azure DevOps or GitHub Actions
- Configure build and test automation





## Deployment
### Set Up Local Development Environment

1. Set up Azure SQL locally:
- Use the SQL Server container in docker-compose
- Create database migrations
- Add connection strings to appsettings.json


2. Set up Azure Service Bus Emulator:
- Configure message queues between services
- Implement publish/subscribe patterns


3. Configure development secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=FhirToHl7;User Id=sa;Password=YourStrongPassword123!"
```


## Key Features to Test

1. Submit a FHIR Resource: Use the API endpoint `/api/fhir` with a sample FHIR Patient resource
2. Monitor Message Flow: Watch the logs to see the message being processed through the services
3. View Messages in Dashboard: See the message in the web UI
4. Resend a Message: Test the resend functionality
5. Toggle Clinic Status: Try enabling/disabling a clinic

The system uses Azure Service Bus for communication between services, so make sure the emulator is running in your Docker environment as specified in the [docker-compose.yml](docker-compose.yml) file.
For further improvements and feature additions, refer to the [Next Steps and Future Improvements document](Next Steps and Future Improvements.md).
