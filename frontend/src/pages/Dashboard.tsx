// File: frontend/src/pages/Dashboard.jsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { makeStyles } from '@material-ui/core/styles';
import { Grid, Paper, Typography } from '@material-ui/core';
import MessageList from '../components/MessageList';
import BatchResendDialog from '../components/BatchResendDialog';
import ClinicSidebar from '../components/ClinicSidebar';

const useStyles = makeStyles((theme) => ({
  root: {
    flexGrow: 1,
    padding: theme.spacing(3),
  },
  title: {
    marginBottom: theme.spacing(3),
  },
  paper: {
    padding: theme.spacing(2),
    height: '100%',
  },
  messageListContainer: {
    minHeight: 'calc(100vh - 180px)',
  },
  clinicSidebarContainer: {
    height: '100%',
  },
}));

const Dashboard = () => {
  const classes = useStyles();
  const navigate = useNavigate();
  
  const [selectedClinicId, setSelectedClinicId] = useState(null);
  const [batchResendOpen, setBatchResendOpen] = useState(false);
  const [selectedMessageIds, setSelectedMessageIds] = useState([]);
  
  const handleClinicSelect = (clinicId) => {
    setSelectedClinicId(clinicId);
    // In a real implementation, this would filter messages by clinic
  };
  
  const handleOpenBatchResend = (messageIds) => {
    setSelectedMessageIds(messageIds);
    setBatchResendOpen(true);
  };
  
  const handleCloseBatchResend = () => {
    setBatchResendOpen(false);
  };
  
  const handleBatchResendComplete = () => {
    // In a real implementation, this would refresh the message list
    setTimeout(() => {
      setBatchResendOpen(false);
      // Refresh messages or show a success notification
    }, 1000);
  };
  
  const handleViewMessageDetails = (messageId) => {
    navigate(`/messages/${messageId}`);
  };
  
  return (
    <div className={classes.root}>
      <Typography variant="h4" className={classes.title}>
        FHIR to HL7 Message Dashboard
      </Typography>
      
      <Grid container spacing={3}>
        <Grid item xs={12} md={3} className={classes.clinicSidebarContainer}>
          <ClinicSidebar 
            onClinicSelect={handleClinicSelect}
            selectedClinicId={selectedClinicId}
          />
        </Grid>
        
        <Grid item xs={12} md={9} className={classes.messageListContainer}>
          <Paper className={classes.paper}>
            <MessageList 
              onOpenBatchResend={handleOpenBatchResend}
              onViewDetails={handleViewMessageDetails}
              selectedClinicId={selectedClinicId}
            />
          </Paper>
        </Grid>
      </Grid>
      
      <BatchResendDialog
        open={batchResendOpen}
        onClose={handleCloseBatchResend}
        messageIds={selectedMessageIds}
        onComplete={handleBatchResendComplete}
      />
    </div>
  );
};

export default Dashboard;
