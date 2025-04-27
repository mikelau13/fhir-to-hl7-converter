// File: frontend/src/components/ClinicSidebar.tsx
import React, { useState, useEffect } from 'react';
import { 
  List, 
  ListItem, 
  ListItemText, 
  ListItemSecondaryAction, 
  Switch, 
  TextField,
  Typography,
  CircularProgress,
  Divider
} from '@material-ui/core';
import { useClinics } from '../context/ClinicContext';

// Define interface for clinic objects
interface Clinic {
  id: string;
  name: string;
  isActive: boolean;
}

export default function ClinicSidebar() {
  const { clinics, loading, error, fetchClinics, toggleClinicStatus } = useClinics();
  const [searchTerm, setSearchTerm] = useState<string>('');
  
  // Fetch clinics on component mount
  useEffect(() => {
    fetchClinics();
  }, [fetchClinics]);
  
  const handleToggle = async (id: string, currentValue: boolean) => {
    await toggleClinicStatus(id, currentValue);
  };
  
  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value);
  };
  
  const filteredClinics = clinics.filter(clinic => 
    clinic.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div>
      <Typography variant="h6" style={{ marginBottom: 16 }}>Clinics</Typography>
      
      <TextField
        label="Search clinics..."
        value={searchTerm}
        onChange={handleSearchChange}
        fullWidth
        margin="normal"
        variant="outlined"
        size="small"
      />
      
      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 20 }}>
          <CircularProgress size={24} />
        </div>
      ) : error ? (
        <Typography color="error" variant="body2">
          Error: {error}
        </Typography>
      ) : clinics.length === 0 ? (
        <Typography variant="body2">
          No clinics found
        </Typography>
      ) : filteredClinics.length === 0 ? (
        <Typography variant="body2">
          No clinics match your search
        </Typography>
      ) : (
        <>
          <Divider style={{ margin: '16px 0 8px 0' }} />
          <List>
            {filteredClinics.map((clinic) => (
              <ListItem key={clinic.id} dense>
                <ListItemText 
                  primary={clinic.name} 
                  secondary={clinic.isActive ? 'Active' : 'Inactive'}
                />
                <ListItemSecondaryAction>
                  <Switch
                    edge="end"
                    checked={clinic.isActive}
                    onChange={() => handleToggle(clinic.id, clinic.isActive)}
                  />
                </ListItemSecondaryAction>
              </ListItem>
            ))}
          </List>
        </>
      )}
    </div>
  );
}