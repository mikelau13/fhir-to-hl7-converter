// File: frontend/src/components/BatchResendDialog.jsx
import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  CircularProgress,
  List,
  ListItem,
  ListItemText,
  Divider,
  Box
} from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import api from '../services/api';

const useStyles = makeStyles((theme) => ({
  dialogContent: {
    minWidth: 400,
    maxWidth: 600,
  },
  messagesList: {
    maxHeight: 300,
    overflow: 'auto',
    marginTop: theme.spacing(2),
    marginBottom: theme.spacing(2),
    border: `1px solid ${theme.palette.divider}`,
    borderRadius: theme.shape.borderRadius,
  },
  loadingBox: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    padding: theme.spacing(3),
  },
  successMessage: {
    backgroundColor: theme.palette.success.light,
    padding: theme.spacing(2),
    borderRadius: theme.shape.borderRadius,
    marginTop: theme.spacing(2),
  },
  errorMessage: {
    backgroundColor: theme.palette.error.light,
    padding: theme.spacing(2),
    borderRadius: theme.shape.borderRadius,
    marginTop: theme.spacing(2),
  },
}));

const BatchResendDialog = ({ open, onClose, messageIds, onComplete }) => {
  const classes = useStyles();
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState(null);

  useEffect(() => {
    if (open && messageIds.length > 0) {
      fetchMessages();
    }
  }, [open, messageIds]);

  const fetchMessages = async () => {
    setLoading(true);
    setResult(null);
    try {
      // In a real implementation, this would fetch message details for all IDs
      // Here we're using a mock implementation
      const messagesData = [];
      for (const id of messageIds) {
        try {
          const response = await api.getMessage(id);
          messagesData.push(response.data);
        } catch (error) {
          console.error(`Error fetching message ${id}:`, error);
        }
      }
      setMessages(messagesData);
    } catch (error) {
      console.error('Error fetching messages:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleResendAll = async () => {
    setSubmitting(true);
    setResult(null);
    try {
      const response = await api.batchResendMessages(messageIds);
      
      // Calculate success/failure counts
      const successCount = response.data.results.filter(r => r.success).length;
      const failureCount = response.data.results.filter(r => !r.success).length;
      
      setResult({
        success: successCount > 0,
        message: `${successCount} message${successCount !== 1 ? 's' : ''} queued for resend successfully.${
          failureCount > 0 ? ` ${failureCount} failed.` : ''
        }`,
        details: response.data.results
      });
      
      if (successCount > 0 && onComplete) {
        setTimeout(() => {
          onComplete();
        }, 2000);
      }
    } catch (error) {
      console.error('Error resending messages:', error);
      setResult({
        success: false,
        message: 'Failed to resend messages. Please try again later.',
        error: error.message
      });
    } finally {
      setSubmitting(false);
    }
  };

  const handleClose = () => {
    if (!submitting) {
      setMessages([]);
      setResult(null);
      onClose();
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md">
      <DialogTitle>Batch Resend</DialogTitle>
      <DialogContent className={classes.dialogContent}>
        <Typography variant="body1">
          You are about to resend {messageIds.length} message{messageIds.length !== 1 ? 's' : ''}.
        </Typography>
        
        {loading ? (
          <Box className={classes.loadingBox}>
            <CircularProgress />
          </Box>
        ) : (
          <>
            <List className={classes.messagesList}>
              {messages.map((message, index) => (
                <React.Fragment key={message.messageId}>
                  {index > 0 && <Divider />}
                  <ListItem>
                    <ListItemText
                      primary={`Message ID: ${message.messageId}`}
                      secondary={`Patient: ${message.patientId} | Type: ${message.messageType} | Status: ${message.status}`}
                    />
                  </ListItem>
                </React.Fragment>
              ))}
              {messages.length === 0 && (
                <ListItem>
                  <ListItemText primary="No message details available" />
                </ListItem>
              )}
            </List>
            
            {result && (
              <Typography
                variant="body1"
                className={result.success ? classes.successMessage : classes.errorMessage}
              >
                {result.message}
              </Typography>
            )}
          </>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={submitting}>
          {result && result.success ? 'Close' : 'Cancel'}
        </Button>
        <Button
          variant="contained"
          color="primary"
          onClick={handleResendAll}
          disabled={loading || submitting || (result && result.success)}
          startIcon={submitting ? <CircularProgress size={20} /> : null}
        >
          {submitting ? 'Resending...' : 'Resend All'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default BatchResendDialog;
