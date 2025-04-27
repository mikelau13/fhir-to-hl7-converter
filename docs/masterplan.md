# FHIR to HL7 Conversion System Masterplan

## 1. Application Overview

### 1.1 Project Objectives
- Create a microservices-based system that converts FHIR R4 STU3 objects to HL7 v2.4 messages
- Forward the converted HL7 messages to the Ontario PCR endpoint
- Log message transmission status and provide resend capabilities
- Allow users to manage and resend messages through a user-friendly interface
- Enable clinic-level controls for message management
- Provide failure notifications via daily digest emails

### 1.2 Target Audience
- Internal staff at an EMR service provider supporting approximately 45 clinics
- These users will access the system through a secure VPN to monitor and manage health data integration

## 2. Core Features & Functionality

### 2.1 Message Conversion
- Transform FHIR R4 STU3 resources to HL7 v2.4 messages
- Initially support ADT message types (A28, A31, and A40 specifically)
- Future phase: Extend support to ORU message types

### 2.2 Message Transmission
- Forward converted HL7 messages to Ontario PCR endpoint
- Handle acknowledgments (ACK/NACK) and track transmission status
- Implement retry logic for failed transmissions

### 2.3 Message Logging
- Log minimal but sufficient metadata for each message
- Track message status (pending, sent, failed)
- Store message content for potential resend
- Implement different retention policies:
  - Successful messages: 3 months retention
  - Failed messages: Indefinite retention until successful resend

### 2.4 Message Resend Interface
- Web-based UI built with React.js
- Enable single message resends with message editing capability
- Support batch resends of multiple messages
- Allow filtering and resending messages by patient ID
- Provide clinic-level toggling for message management
- Combined filtering capabilities across multiple fields

### 2.5 Notification System
- Generate daily digest emails for:
  - Connectivity errors
  - EACK/NACK responses
  - Outstanding messages from the previous day
- Send emails via SMTP (smtp.outlook.com)

## 3. Technical Architecture

### 3.1 High-Level Architecture

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ FHIR Input  │────>│ Conversion  │────>│ Transmission │────>│ Ontario PCR │
│  Service    │     │  Service    │     │   Service    │     │  Endpoint   │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                           │                   │
                           ▼                   ▼
                    ┌─────────────┐     ┌─────────────┐
                    │ Message Log │<────│ Notification │
                    │  Database   │     │   Service    │
                    └─────────────┘     └─────────────┘
                           ▲                   │
                           │                   ▼
                    ┌─────────────┐     ┌─────────────┐
                    │   Resend    │     │ Email Digest │
                    │ UI Service  │     │   Service    │
                    └─────────────┘     └─────────────┘
```

### 3.2 Recommended Technical Stack

#### 3.2.1 Backend
- **Framework**: .NET Core 8.0 for microservices
- **API Gateway**: Azure API Management or YARP
- **Message Queue**: Azure Service Bus (for decoupling services)
- **Database**: 
  - Azure SQL Database for message logging and metadata
  - Consider Azure Cosmos DB if document storage would be beneficial
- **Authentication**: Azure AD for internal staff authentication
- **Service Mesh**: Linkerd (for Kubernetes deployment)

#### 3.2.2 Frontend
- **Framework**: React.js with TypeScript
- **UI Components**: Material-UI or Ant Design
- **State Management**: React Context or Redux
- **HTTP Client**: Axios
- **Form Handling**: Formik with Yup validation

#### 3.2.3 Infrastructure
- **Containerization**: Docker
- **Orchestration**: Kubernetes (AKS for production, Minikube for development)
- **CI/CD**: Azure DevOps or GitHub Actions
- **Monitoring**: Application Insights
- **Logging**: Azure Log Analytics or ELK Stack

### 3.3 Microservices Breakdown

1. **FHIR Receiver Service**
   - Receives incoming FHIR resources
   - Validates incoming messages
   - Routes to appropriate processing pipeline

2. **Conversion Service**
   - Transforms FHIR R4 STU3 to HL7 v2.4
   - Focuses on ADT messages (A28, A31, A40)
   - Provides extensibility for future message types (ORU)

3. **Transmission Service**
   - Sends HL7 messages to Ontario PCR
   - Handles acknowledgments
   - Implements retry logic

4. **Logging Service**
   - Records message metadata and content
   - Implements retention policies
   - Provides query capabilities for the UI

5. **Notification Service**
   - Monitors for errors and failed transmissions
   - Generates daily digest emails
   - Sends via SMTP

6. **Resend API Service**
   - Provides REST endpoints for the UI
   - Manages clinic filtering
   - Supports message editing and resending

7. **UI Application**
   - React-based frontend
   - Secure access via VPN
   - Message management capabilities

## 4. Conceptual Data Model

### 4.1 Core Entities

#### 4.1.1 Message
- MessageId: unique identifier
- PatientId: reference to patient
- ClinicId: reference to source clinic
- OriginalFhirContent: the original FHIR resource
- ConvertedHl7Content: the converted HL7 message
- MessageType: type of message (A28, A31, A40)
- Status: current status (Pending, Sent, Failed)
- CreatedAt: timestamp of message creation
- LastUpdatedAt: timestamp of last status change
- ResendCount: number of resend attempts
- RetentionDate: calculated date for message purging

#### 4.1.2 Clinic
- ClinicId: unique identifier
- ClinicName: display name
- IsActive: toggle state for message processing
- Settings: clinic-specific configuration

#### 4.1.3 Transmission
- TransmissionId: unique identifier
- MessageId: reference to the message
- AttemptedAt: timestamp of transmission attempt
- Status: result (Success, Failed)
- AcknowledgmentType: type of acknowledgment received
- ErrorDetails: information about failures
- ResponseTime: performance metric

#### 4.1.4 Digest
- DigestId: unique identifier
- GeneratedDate: date of digest creation
- ErrorCount: number of errors reported
- OutstandingCount: number of messages pending
- SentStatus: whether email was successfully sent
- Recipients: list of email recipients

## 5. User Interface Design

### 5.1 Main Interface Components

#### 5.1.1 Message Management Dashboard
- **Unified Filter Panel**
  - Combined filters in a single section
  - Optional filter fields (Patient ID, Status, Date Range)
  - Apply/Clear filter buttons
  - All filters can be used independently or in combination

- **Message List**
  - Checkboxes for message selection
  - "Select all" option in header
  - Columns for Message ID, Patient ID, Timestamp, Status
  - Action column with individual Resend buttons
  - Visual status indicators

- **Batch Operations**
  - Batch Resend button with selection counter
  - Confirmation dialog for batch operations
  - Options to edit before resending or resend immediately

#### 5.1.2 Clinic Management Panel
- Toggle switches for each clinic
- Search/filter for clinic list
- Status indicators for clinic connectivity
- Message volume metrics

#### 5.1.3 Message Editor
- Raw HL7 message editing capabilities
- Syntax highlighting
- Validation feedback
- Send/cancel actions

#### 5.1.4 Batch Resend Interface
- Selection summary with message details
- Options to cancel, edit before resend, or resend immediately
- Batch processing status feedback

### 5.2 Design Principles
- Clean, minimal interface focused on functionality
- Clear status indicators using color coding
- Responsive design for different device sizes
- Efficient workflows for common operations
- Consistent layout and interaction patterns

## 6. Security Considerations

### 6.1 Data Protection
- Implement HIPAA-compliant data handling
- Encrypt data at rest and in transit
- Implement appropriate access controls
- Regular security audits and testing

### 6.2 Authentication & Authorization
- Secure access via VPN
- Role-based access control
- Audit logging for all user actions
- Session management and timeouts

### 6.3 Message Security
- Message integrity validation
- Secure transmission protocols
- Protection against message tampering
- Protection against injection attacks in message editor

## 7. Development Phases

### 7.1 Phase 1: Core Functionality
- Implement FHIR to HL7 conversion for ADT messages (A28, A31, A40)
- Create basic message transmission pipeline
- Develop essential logging functionality
- Implement minimal UI for message viewing

### 7.2 Phase 2: Enhanced Management
- Develop complete message resend functionality
- Implement clinic toggling
- Create message editing capabilities
- Build notification system for daily digests

### 7.3 Phase 3: Production Readiness
- Implement comprehensive error handling
- Enhance security features
- Optimize performance
- Deploy to production environment (Azure K8s)

### 7.4 Phase 4: Extension (Future)
- Add support for ORU message types
- Enhance reporting capabilities
- Implement advanced monitoring

## 8. Potential Challenges & Solutions

### 8.1 Message Transformation Accuracy
- **Challenge**: Ensuring accurate FHIR to HL7 conversions
- **Solution**: Implement extensive validation rules and reference known good examples

### 8.2 Message Delivery Reliability
- **Challenge**: Ensuring messages reach Ontario PCR reliably
- **Solution**: Implement robust retry logic and monitoring

### 8.3 Performance at Scale
- **Challenge**: Maintaining performance as message volume grows
- **Solution**: Design for horizontal scaling and implement performance benchmarks

### 8.4 HIPAA Compliance
- **Challenge**: Ensuring all aspects of the system are HIPAA compliant
- **Solution**: Regular compliance audits and security reviews

### 8.5 Message Editing Safety
- **Challenge**: Preventing errors during manual message editing
- **Solution**: Implement validation and preview features in the editor

### 8.6 Batch Processing Reliability
- **Challenge**: Ensuring batch operations complete successfully
- **Solution**: Implement transaction management and detailed status reporting

## 9. Future Expansion Possibilities

### 9.1 Additional Message Types
- Extend beyond ADT to support more HL7 message types
- Support additional FHIR resource types

### 9.2 Advanced Analytics
- Message volume and pattern analysis
- Error trend identification
- Performance optimization recommendations

### 9.3 Bidirectional Communication
- Support for inbound messages from Ontario PCR
- Two-way synchronization capabilities

### 9.4 Enhanced UI Capabilities
- Advanced filtering and search
- Custom dashboards
- Reporting and export features

## 10. Development & Testing Approach

### 10.1 Development Environment
- Local Minikube for Kubernetes testing
- Docker-based development environment
- CI/CD pipeline for automated builds and tests

### 10.2 Testing Strategy
- Unit testing for individual components
- Integration testing for service interactions
- End-to-end testing for complete workflows
- Performance testing for throughput validation
- Security testing for vulnerability detection

### 10.3 Deployment Strategy
- Containerized deployment to Azure Kubernetes Service
- Blue-green deployment for zero-downtime updates
- Environment-specific configuration management
- Automated rollback capabilities

## 11. User Interface Implementation Details

### 11.1 Message List View
- Display messages in a paginated table format
- Support sorting by column headers
- Implement selection mechanism for batch operations
- Provide visual indicators for message status
- Show appropriate actions based on message state

### 11.2 Filter Implementation
- Use a unified filter panel with all criteria in one section
- Implement client-side filtering for responsive feedback
- Support server-side filtering for large datasets
- Persist filter state across user sessions
- Provide clear visual feedback when filters are active

### 11.3 Message Editing Interface
- Implement a modal dialog for message editing
- Provide syntax highlighting for HL7 content
- Include basic validation to prevent common formatting errors
- Support keyboard shortcuts for efficient editing
- Provide a preview/diff option to see changes

### 11.4 Batch Processing
- Support multi-select through checkboxes
- Provide batch resend with or without editing
- Include confirmation steps for batch operations
- Show progress during batch processing
- Provide detailed results after batch completion
