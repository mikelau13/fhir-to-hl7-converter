// File: frontend/src/components/MessageDetails.jsx
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Paper,
  Typography,
  Button,
  Grid,
  TextField,
  Divider,
  Chip,
  Box,
  CircularProgress,
  Tabs,
  Tab
} from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import { formatDistanceToNow } from 'date-fns';
import api from '../services/api';

const useStyles = makeStyles((theme) => ({
  paper: {
    padding: theme.spacing(3),
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: theme.spacing(3),
  },
  messageId: {
    wordBreak: 'break-all',
  },
  divider: {
    margin: theme.spacing(3, 0),
  },
  contentField: {
    fontFamily: 'monospace',
    marginTop: theme.spacing(2),
  },
  actionsContainer: {
    display: 'flex',
    justifyContent: 'flex-end',
    gap: theme.spacing(1),
    marginTop: theme.spacing(2),
  },
  loadingContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    minHeight: 400,
  },
  statusPending: {
    backgroundColor: theme.palette.warning.light,
  },
  statusSent: {
    backgroundColor: theme.palette.success.light,
  },
  statusFailed: {
    backgroundColor: theme.palette.error.light,
  },
  metadataGrid: {
    marginBottom: theme.spacing(2),
  },
  tabContent: {
    marginTop: theme.spacing(2),
  },
}));

const MessageDetails = () => {
  const classes = useStyles();
  const { id } = useParams();
  const navigate = useNavigate();
  
  const [message, setMessage] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editMode, setEditMode] = useState(false);
  const [editedContent, setEditedContent] = useState('');
  const [activeTab, setActiveTab] = useState(0);
  
  useEffect(() => {
    fetchMessage();
  }, [id]);
  
  const fetchMessage = async () => {
    setLoading(true);
    try {
      const response = await api.getMessage(id);
      setMessage(response.data);
      setEditedContent(response.data.hl7Content);
    } catch (error) {
      console.error(`Error fetching message ${id}:`, error);
      // In a real app, show an error notification
    } finally {
      setLoading(false);
    }
  };
  
  const handleBack = () => {
    navigate(-1);
  };
  
  const handleEdit = () => {
    setEditMode(true);
  };
  
  const handleCancel = () => {
    setEditMode(false);
    setEditedContent(message.hl7Content);
  };
  
  const handleSave = async () => {
    try {
      await api.updateMessageContent(id, { updatedContent: editedContent });
      fetchMessage(); // Refresh the message
      setEditMode(false);
    } catch (error) {
      console.error(`Error updating message ${id}:`, error);
      // In a real app, show an error notification
    }
  };
  
  const handleResend = async () => {
    try {
      await api.resendMessage(id, { editBeforeResend: false });
      fetchMessage(); // Refresh the message
    } catch (error) {
      console.error(`Error resending message ${id}:`, error);
      // In a real app, show an error notification
    }
  };
  
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  const formatTimestamp = (timestamp) => {
    try {
      return formatDistanceToNow(new Date(timestamp), { addSuffix: true });
    } catch (error) {
      return timestamp;
    }
  };
  
  const renderStatus = (status) => {
    let className;
    switch (status) {
      case 'Pending':
        className = classes.statusPending;
        break;
      case 'Sent':
        className = classes.statusSent;
        break;
      case 'Failed':
        className = classes.statusFailed;
        break;
      default:
        className = '';
    }

    return <Chip label={status} className={className} />;
  };
  
  if (loading) {
    return (
      <div className={classes.loadingContainer}>
        <CircularProgress />
      </div>
    );
  }
  
  if (!message) {
    return (
      <Typography variant="h5" align="center">
        Message not found or error loading message.
      </Typography>
    );
  }
  
  return (
    <Paper className={classes.paper}>
      <div className={classes.header}>
        <Typography variant="h5" className={classes.messageId}>
          Message Details: {message.messageId}
        </Typography>
        <Button variant="outlined" onClick={handleBack}>
          Back
        </Button>
      </div>
      
      <Grid container spacing={2} className={classes.metadataGrid}>
        <Grid item xs={12} md={4}>
          <Typography variant="body1">
            <strong>Patient ID:</strong> {message.patientId}
          </Typography>
        </Grid>
        <Grid item xs={12} md={4}>
          <Typography variant="body1">
            <strong>Clinic:</strong> {message.clinicName}
          </Typography>
        </Grid>
        <Grid item xs={12} md={4}>
          <Typography variant="body1">
            <strong>Status:</strong> {renderStatus(message.status)}
          </Typography>
        </Grid>
        <Grid item xs={12} md={4}>
          <Typography variant="body1">
            <strong>Message Type:</strong> {message.messageType}
          </Typography>
        </Grid>
        <Grid item xs={12} md={4}>
          <Typography variant="body1">
            <strong>Created:</strong> {formatTimestamp(message.createdAt)}
          </Typography>
        </Grid>
        <Grid item xs={12} md={4}>
          <Typography variant="body1">
            <strong>Last Updated:</strong> {formatTimestamp(message.lastUpdatedAt)}
          </Typography>
        </Grid>
      </Grid>
      
      <Divider className={classes.divider} />
      
      <Tabs value={activeTab} onChange={handleTabChange} indicatorColor="primary" textColor="primary">
        <Tab label="HL7 Content" />
        <Tab label="FHIR Content" />
      </Tabs>
      
      <div className={classes.tabContent}>
        {activeTab === 0 && (
          <div>
            <Typography variant="h6">HL7 Content</Typography>
            <TextField
              multiline
              rows={20}
              variant="outlined"
              fullWidth
              value={editMode ? editedContent : message.hl7Content}
              onChange={(e) => setEditedContent(e.target.value)}
              disabled={!editMode}
              className={classes.contentField}
            />
          </div>
        )}
        
        {activeTab === 1 && (
          <div>
            <Typography variant="h6">Original FHIR Content</Typography>
            <TextField
              multiline
              rows={20}
              variant="outlined"
              fullWidth
              value={message.originalFhirContent || 'No FHIR content available'}
              disabled
              className={classes.contentField}
            />
          </div>
        )}
      </div>
      
      <div className={classes.actionsContainer}>
        {editMode ? (
          <>
            <Button variant="outlined" onClick={handleCancel}>
              Cancel
            </Button>
            <Button variant="contained" color="primary" onClick={handleSave}>
              Save
            </Button>
          </>
        ) : (
          <>
            <Button variant="outlined" onClick={handleEdit}>
              Edit
            </Button>
            <Button 
              variant="contained" 
              color="primary" 
              onClick={handleResend}
              disabled={message.status === 'Pending'}
            >
              Resend
            </Button>
          </>
        )}
      </div>
    </Paper>
  );
};

export default MessageDetails;
