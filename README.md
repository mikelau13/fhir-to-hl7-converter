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

To run locally (not in container):
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
TODO: Add development instructions

## Deployment
TODO: Add deployment instructions