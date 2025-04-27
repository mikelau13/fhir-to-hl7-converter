// File: frontend/src/pages/ClinicManagement.tsx
import React, { useState, useEffect } from 'react';
import { 
  Paper, 
  Typography, 
  Table, 
  TableBody, 
  TableCell, 
  TableContainer, 
  TableHead, 
  TableRow,
  Switch
} from '@material-ui/core';
import { useClinics } from '../context/ClinicContext';

// Define interface for clinic data
interface Clinic {
  id: string;
  name: string;
  isActive: boolean;
}

export default function ClinicManagement() {
  const { clinics, loading, error, fetchClinics, toggleClinicStatus } = useClinics();
  
  // Fetch clinics on component mount
  useEffect(() => {
    fetchClinics();
  }, [fetchClinics]);
  
  const handleToggle = async (id: string, currentValue: boolean) => {
    // Update clinic status via context
    const success = await toggleClinicStatus(id, currentValue);
    if (!success) {
      // Handle error case if needed
      console.error('Failed to update clinic status');
    }
  };

  if (loading) {
    return <Typography>Loading clinics...</Typography>;
  }

  if (error) {
    return <Typography color="error">Error: {error}</Typography>;
  }

  return (
    <Paper style={{ padding: 24 }}>
      <Typography variant="h5" style={{ marginBottom: 16 }}>Clinic Management</Typography>
      
      {clinics.length === 0 ? (
        <Typography>No clinics found</Typography>
      ) : (
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Clinic ID</TableCell>
                <TableCell>Name</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Toggle</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {clinics.map((clinic) => (
                <TableRow key={clinic.id}>
                  <TableCell>{clinic.id}</TableCell>
                  <TableCell>{clinic.name}</TableCell>
                  <TableCell>{clinic.isActive ? 'Active' : 'Inactive'}</TableCell>
                  <TableCell>
                    <Switch
                      checked={clinic.isActive}
                      onChange={() => handleToggle(clinic.id, clinic.isActive)}
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Paper>
  );
}