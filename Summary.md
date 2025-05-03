Backend Services

FHIR Receiver Service - Entry point for FHIR resources with validation
Conversion Service - Transforms FHIR to HL7 for ADT messages (A28, A31, A40)
Transmission Service - Sends HL7 messages to Ontario PCR with retry logic
Logging Service - Records message metadata and provides querying capabilities
Notification Service - Generates daily digest emails for errors and pending messages
Resend API Service - Provides REST endpoints for the UI

Frontend Components

Dashboard - Main page with message list and clinic filtering
Message Details - View for inspecting and editing message content
Batch Resend Dialog - For resending multiple messages
Clinic Management - For toggling clinic activation status
