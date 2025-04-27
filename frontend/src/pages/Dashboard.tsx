// File: frontend/src/pages/Dashboard.tsx
import React, { useState, useEffect } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { Grid, Paper } from '@material-ui/core';
import FilterPanel from '../components/FilterPanel';
import MessageList from '../components/MessageList';
import ClinicSidebar from '../components/ClinicSidebar';
import BatchResendDialog from '../components/BatchResendDialog';
import { useMessages } from '../context/MessageContext';

// Define interfaces
interface FilterValues {
  patientId: string;
  status: string;
  fromDate?: string;
  toDate?: string;
}

const useStyles = makeStyles((theme) => ({
  root: {
    flexGrow: 1,
  },
  paper: {
    padding: theme.spacing(2),
    color: theme.palette.text.secondary,
  },
}));

export default function Dashboard() {
  const classes = useStyles();
  const { messages, loading, error, fetchMessages, batchResend } = useMessages();
  
  const [filters, setFilters] = useState<FilterValues>({
    patientId: '',
    status: ''
  });
  
  const [selectedMessages, setSelectedMessages] = useState<string[]>([]);
  const [batchDialogOpen, setBatchDialogOpen] = useState<boolean>(false);
  
  // Fetch messages on mount and when filters change
  useEffect(() => {
    fetchMessages(filters);
  }, [fetchMessages]);
  
  const handleFilterChange = (newFilters: FilterValues) => {
    setFilters(newFilters);
    fetchMessages(newFilters);
  };
  
  const handleSelectionChange = (selected: string[]) => {
    setSelectedMessages(selected);
  };
  
  const handleBatchResendOpen = () => {
    setBatchDialogOpen(true);
  };
  
  const handleBatchResendClose = () => {
    setBatchDialogOpen(false);
  };
  
  const handleBatchResend = async () => {
    const success = await batchResend(selectedMessages);
    if (success) {
      setSelectedMessages([]);
      fetchMessages(filters);
    }
    setBatchDialogOpen(false);
  };
  
  const handleEditBeforeResend = () => {
    // Navigate to edit page or open edit dialog
    // For now, just close the dialog
    setBatchDialogOpen(false);
  };
  
  const selectedMessageObjects = messages.filter(message => 
    selectedMessages.includes(message.id)
  );

  return (
    <div className={classes.root}>
      <Grid container spacing={3}>
        <Grid item xs={12} md={3}>
          <Paper className={classes.paper}>
            <ClinicSidebar />
          </Paper>
        </Grid>
        <Grid item xs={12} md={9}>
          <Paper className={classes.paper}>
            <FilterPanel 
              filters={filters} 
              onFilterChange={handleFilterChange} 
            />
            <MessageList 
              selectedMessages={selectedMessages}
              onSelectionChange={handleSelectionChange}
              onBatchResend={handleBatchResendOpen}
            />
          </Paper>
        </Grid>
      </Grid>
      
      <BatchResendDialog
        open={batchDialogOpen}
        onClose={handleBatchResendClose}
        messages={selectedMessageObjects}
        onResend={handleBatchResend}
        onEditBeforeResend={handleEditBeforeResend}
      />
    </div>
  );
}