// File: frontend/src/components/ClinicSidebar.jsx
import React, { useState, useEffect } from 'react';
import {
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Switch,
  TextField,
  Typography,
  Paper,
  CircularProgress,
  Divider,
  Box
} from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import api from '../services/api';

const useStyles = makeStyles((theme) => ({
  root: {
    height: '100%',
  },
  header: {
    padding: theme.spacing(2),
    backgroundColor: theme.palette.primary.main,
    color: theme.palette.primary.contrastText,
  },
  searchBox: {
    padding: theme.spacing(2),
  },
  listContainer: {
    maxHeight: 'calc(100vh - 250px)',
    overflow: 'auto',
  },
  loadingContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    padding: theme.spacing(4),
  },
  clinicActive: {
    '& .MuiListItemText-primary': {
      fontWeight: 'bold',
    },
  },
  clinicInactive: {
    '& .MuiListItemText-primary': {
      color: theme.palette.text.disabled,
    },
  },
  noResults: {
    textAlign: 'center',
    padding: theme.spacing(2),
  },
}));

const ClinicSidebar = ({ onClinicSelect, selectedClinicId }) => {
  const classes = useStyles();
  const [clinics, setClinics] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [toggleLoading, setToggleLoading] = useState(false);

  useEffect(() => {
    fetchClinics();
  }, []);

  const fetchClinics = async () => {
    setLoading(true);
    try {
      const response = await api.getClinics();
      setClinics(response.data);
    } catch (error) {
      console.error('Error fetching clinics:', error);
      // In a real app, show an error notification
    } finally {
      setLoading(false);
    }
  };

  const handleSearchChange = (event) => {
    setSearchTerm(event.target.value);
  };

  const handleToggle = async (clinic) => {
    // Prevent toggling while another toggle is in progress
    if (toggleLoading) return;
    
    const clinicId = clinic.clinicId;
    const newStatus = !clinic.isActive;
    
    // Optimistically update UI
    setClinics(clinics.map(c => 
      c.clinicId === clinicId 
        ? { ...c, isActive: newStatus }
        : c
    ));
    
    setToggleLoading(true);
    try {
      await api.updateClinicStatus(clinicId, { isActive: newStatus });
      // UI already updated optimistically
    } catch (error) {
      console.error(`Error updating clinic ${clinicId} status:`, error);
      // Revert the optimistic update
      setClinics(clinics.map(c => 
        c.clinicId === clinicId 
          ? { ...c, isActive: !newStatus }
          : c
      ));
      // In a real app, show an error notification
    } finally {
      setToggleLoading(false);
    }
  };

  const handleClinicSelect = (clinicId) => {
    if (onClinicSelect) {
      onClinicSelect(clinicId);
    }
  };

  const filteredClinics = clinics.filter(clinic =>
    clinic.clinicName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    clinic.clinicId.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <Paper className={classes.root}>
      <div className={classes.header}>
        <Typography variant="h6">Clinics</Typography>
      </div>
      
      <div className={classes.searchBox}>
        <TextField
          label="Search clinics"
          variant="outlined"
          size="small"
          fullWidth
          value={searchTerm}
          onChange={handleSearchChange}
        />
      </div>
      
      <Divider />
      
      {loading ? (
        <div className={classes.loadingContainer}>
          <CircularProgress />
        </div>
      ) : (
        <div className={classes.listContainer}>
          <List>
            {filteredClinics.length > 0 ? (
              filteredClinics.map((clinic) => (
                <ListItem
                  key={clinic.clinicId}
                  button
                  onClick={() => handleClinicSelect(clinic.clinicId)}
                  selected={selectedClinicId === clinic.clinicId}
                  className={clinic.isActive ? classes.clinicActive : classes.clinicInactive}
                >
                  <ListItemText
                    primary={clinic.clinicName}
                    secondary={`ID: ${clinic.clinicId}`}
                  />
                  <ListItemSecondaryAction>
                    <Switch
                      edge="end"
                      checked={clinic.isActive}
                      onChange={() => handleToggle(clinic)}
                      disabled={toggleLoading}
                    />
                  </ListItemSecondaryAction>
                </ListItem>
              ))
            ) : (
              <Box className={classes.noResults}>
                <Typography variant="body2" color="textSecondary">
                  No clinics match your search
                </Typography>
              </Box>
            )}
          </List>
        </div>
      )}
    </Paper>
  );
};

export default ClinicSidebar;
