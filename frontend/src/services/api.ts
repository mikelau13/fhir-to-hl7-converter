// File: frontend/src/services/api.ts
import axios, { AxiosError } from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5006/api';

// Define interfaces for API data types
interface Message {
  id: string;
  patientId: string;
  clinicId: string;
  messageType: string;
  status: string;
  createdAt: string;
  hl7Content: string;
}

interface Clinic {
  clinicId: string;
  clinicName: string;
  isActive: boolean;
}

interface MessageFilter {
  patientId?: string;
  status?: string;
  clinicId?: string;
  fromDate?: string;
  toDate?: string;
}

// We'll keep FilterParams for backward compatibility but make it extend MessageFilter
export interface FilterParams extends MessageFilter {
  fromDate?: string;
  toDate?: string;
}

interface ResendOptions {
  editBeforeResend?: boolean;
  updatedContent?: string;
}

// Create axios instance with default config
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Error handling helper
const handleApiError = (error: unknown): never => {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError;
    if (axiosError.response) {
      throw new Error(`API Error: ${axiosError.response.status} ${axiosError.response.statusText}`);
    } else if (axiosError.request) {
      throw new Error('No response received from server. Please check your network connection.');
    }
  }
  throw new Error('An unexpected error occurred');
};

// Message related API calls
export const getMessages = async (filters: MessageFilter = {}): Promise<Message[]> => {
  try {
    const response = await api.get('/messages', { params: filters });
    return response.data;
  } catch (error: unknown) {
    return handleApiError(error);
  }
};

export const getMessage = async (id: string): Promise<Message> => {
  try {
    const response = await api.get(`/messages/${id}`);
    return response.data;
  } catch (error: unknown) {
    return handleApiError(error);
  }
};

export const resendMessage = async (id: string, options: ResendOptions = {}): Promise<any> => {
  try {
    const response = await api.post(`/messages/${id}/resend`, options);
    return response.data;
  } catch (error: unknown) {
    return handleApiError(error);
  }
};

export const batchResendMessages = async (messageIds: string[], options: ResendOptions = {}): Promise<any> => {
  try {
    const response = await api.post('/messages/batch-resend', {
      messageIds,
      ...options
    });
    return response.data;
  } catch (error: unknown) {
    return handleApiError(error);
  }
};

export const updateMessageContent = async (id: string, content: string): Promise<any> => {
  try {
    const response = await api.put(`/messages/${id}/content`, {
      updatedContent: content
    });
    return response.data;
  } catch (error: unknown) {
    return handleApiError(error);
  }
};

// Clinic related API calls
export const getClinics = async (): Promise<Clinic[]> => {
  try {
    const response = await api.get('/clinics');
    return response.data;
  } catch (error: unknown) {
    return handleApiError(error);
  }
};

export const updateClinicStatus = async (id: string, isActive: boolean): Promise<any> => {
  try {
    const response = await api.put(`/clinics/${id}/status`, {
      isActive
    });
    return response.data;
  } catch (error: unknown) {
    return handleApiError(error);
  }
};

export default api;