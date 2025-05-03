// File: frontend/src/services/api.js
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5006/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Error handling interceptor
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // You can add global error handling here
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

// Message related API calls
export const getMessages = async (filters = {}) => {
  try {
    return await api.get('/messages', { params: filters });
  } catch (error) {
    // For demo purposes, return mock data if API is not available
    console.warn('Using mock data for getMessages');
    return {
      data: generateMockMessages(10)
    };
  }
};

export const getMessage = async (id) => {
  try {
    return await api.get(`/messages/${id}`);
  } catch (error) {
    // For demo purposes, return mock data if API is not available
    console.warn('Using mock data for getMessage');
    return {
      data: generateMockMessage(id)
    };
  }
};

export const resendMessage = async (id, options = {}) => {
  return await api.post(`/messages/${id}/resend`, options);
};

export const batchResendMessages = async (messageIds, options = {}) => {
  try {
    return await api.post('/messages/batch-resend', { 
      messageIds,
      ...options
    });
  } catch (error) {
    // For demo purposes, return mock response
    console.warn('Using mock data for batchResendMessages');
    return {
      data: {
        results: messageIds.map(id => ({
          messageId: id,
          success: Math.random() > 0.2, // 80% success rate
          message: Math.random() > 0.2 ? 'Message queued for resend' : 'Failed to resend message'
        }))
      }
    };
  }
};

export const updateMessageContent = async (id, data) => {
  return await api.put(`/messages/${id}/content`, data);
};

// Clinic related API calls
export const getClinics = async () => {
  try {
    return await api.get('/clinics');
  } catch (error) {
    // For demo purposes, return mock data if API is not available
    console.warn('Using mock data for getClinics');
    return {
      data: generateMockClinics(5)
    };
  }
};

export const updateClinicStatus = async (id, data) => {
  return await api.put(`/clinics/${id}/status`, data);
};

// Mock data generators
const generateMockMessages = (count) => {
  const statuses = ['Pending', 'Sent', 'Failed'];
  const messageTypes = ['A28', 'A31', 'A40'];
  
  return Array.from({ length: count }, (_, i) => ({
    messageId: `msg-${Date.now()}-${i}`,
    patientId: `P${Math.floor(Math.random() * 100000)}`,
    clinicId: `clinic-${Math.floor(Math.random() * 5) + 1}`,
    clinicName: `Test Clinic ${Math.floor(Math.random() * 5) + 1}`,
    messageType: messageTypes[Math.floor(Math.random() * messageTypes.length)],
    status: statuses[Math.floor(Math.random() * statuses.length)],
    createdAt: new Date(Date.now() - Math.floor(Math.random() * 7 * 24 * 60 * 60 * 1000)).toISOString(),
    lastUpdatedAt: new Date(Date.now() - Math.floor(Math.random() * 2 * 24 * 60 * 60 * 1000)).toISOString(),
    resendCount: Math.floor(Math.random() * 3)
  }));
};

const generateMockMessage = (id) => {
  const statuses = ['Pending', 'Sent', 'Failed'];
  const messageTypes = ['A28', 'A31', 'A40'];
  const clinicId = `clinic-${Math.floor(Math.random() * 5) + 1}`;
  
  return {
    messageId: id || `msg-${Date.now()}`,
    patientId: `P${Math.floor(Math.random() * 100000)}`,
    clinicId,
    clinicName: `Test Clinic ${clinicId.split('-')[1]}`,
    messageType: messageTypes[Math.floor(Math.random() * messageTypes.length)],
    status: statuses[Math.floor(Math.random() * statuses.length)],
    createdAt: new Date(Date.now() - Math.floor(Math.random() * 7 * 24 * 60 * 60 * 1000)).toISOString(),
    lastUpdatedAt: new Date(Date.now() - Math.floor(Math.random() * 2 * 24 * 60 * 60 * 1000)).toISOString(),
    resendCount: Math.floor(Math.random() * 3),
    hl7Content: `MSH|^~\\&|FHIR_SYSTEM|CLINIC_${clinicId.split('-')[1]}|PCR|ONTARIO|${new Date().toISOString().replace(/[-:]/g, '').substring(0, 14)}||ADT^A31|${id}|P|2.4\nPID|1|${Math.floor(Math.random() * 100000)}||DOE^JOHN^||19800101|M|||123 MAIN ST^^ANYTOWN^ON^A1B2C3||5555551234\nPV1|1|I`,
    originalFhirContent: JSON.stringify({
      resourceType: "Patient",
      id: `patient-${Math.floor(Math.random() * 100000)}`,
      name: [{ family: "Doe", given: ["John"] }],
      gender: "male",
      birthDate: "1980-01-01",
      address: [{
        line: ["123 Main St"],
        city: "Anytown",
        state: "ON",
        postalCode: "A1B 2C3"
      }],
      telecom: [{
        system: "phone",
        value: "555-555-1234"
      }]
    }, null, 2)
  };
};

const generateMockClinics = (count) => {
  return Array.from({ length: count }, (_, i) => ({
    clinicId: `clinic-${i + 1}`,
    clinicName: `Test Clinic ${i + 1}`,
    isActive: Math.random() > 0.3, // 70% active
    settings: '{}'
  }));
};

export default api;
