// frontend/src/context/ClinicContext.tsx
import React, { createContext, useContext, useState } from 'react';
import type { Clinic } from '../types'; // Use explicit type import
import * as api from '../services/api';

interface ClinicContextType {
  clinics: Clinic[];
  loading: boolean;
  error: string | null;
  fetchClinics: () => Promise<void>;
  toggleClinicStatus: (id: string, currentStatus: boolean) => Promise<boolean>;
}

const ClinicContext = createContext<ClinicContextType | null>(null);

export const ClinicProvider: React.FC<{children: React.ReactNode}> = ({ children }) => {
  const [clinics, setClinics] = useState<Clinic[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  const fetchClinics = async () => {
    setLoading(true);
    try {
      const response = await api.getClinics();
      // Explicitly map the response to ensure type compatibility
      const typedClinics: Clinic[] = response.map((clinic: any) => ({
        id: clinic.id || clinic.clinicId,
        name: clinic.name || clinic.clinicName,
        isActive: clinic.isActive
      }));
      setClinics(typedClinics);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  };
  
  const toggleClinicStatus = async (id: string, currentStatus: boolean) => {
    try {
      await api.updateClinicStatus(id, !currentStatus);
      setClinics(prevClinics => 
        prevClinics.map(clinic => 
          clinic.id === id ? { ...clinic, isActive: !currentStatus } : clinic
        )
      );
      return true;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
      return false;
    }
  };
  
  return (
    <ClinicContext.Provider value={{
      clinics,
      loading,
      error,
      fetchClinics,
      toggleClinicStatus
    }}>
      {children}
    </ClinicContext.Provider>
  );
};

export const useClinics = (): ClinicContextType => {
  const context = useContext(ClinicContext);
  if (!context) {
    throw new Error('useClinics must be used within a ClinicProvider');
  }
  return context;
};