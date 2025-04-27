// frontend/src/types/index.ts
export interface Clinic {
    id: string;
    name: string;
    isActive: boolean;
  }
  
  export interface Message {
    id: string;
    patientId: string;
    clinicId: string;
    messageType: string;
    status: string;
    timestamp: string;
    hl7Content: string;
  }

  export interface MessageFilter {
    patientId?: string;
    status?: string;
    clinicId?: string;
    fromDate?: string;
    toDate?: string;
  }
  
  export interface FilterParams {
    patientId: string;
    status: string;
    clinicId: string;
    fromDate?: string;
    toDate?: string;
  }