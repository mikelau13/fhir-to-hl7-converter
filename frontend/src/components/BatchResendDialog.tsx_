// File: frontend/src/components/BatchResendDialog.tsx
import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  List,
  ListItem,
  ListItemText
} from '@material-ui/core';

// Define interfaces for component props
interface Message {
  id: string;
  patientId: string;
  status: string;
}

interface BatchResendDialogProps {
  open: boolean;
  onClose: () => void;
  messages: Message[];
  onResend: () => void;
  onEditBeforeResend: () => void;
}

export default function BatchResendDialog({ 
  open, 
  onClose, 
  messages, 
  onResend, 
  onEditBeforeResend 
}: BatchResendDialogProps) {
  if (!messages || messages.length === 0) {
    return null;
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Batch Resend Confirmation</DialogTitle>
      <DialogContent>
        <Typography gutterBottom>
          You are about to resend {messages.length} messages:
        </Typography>
        
        <List>
          {messages.map(message => (
            <ListItem key={message.id}>
              <ListItemText 
                primary={`${message.id} (Patient: ${message.patientId}, Status: ${message.status})`}
              />
            </ListItem>
          ))}
        </List>
        
        <Typography variant="subtitle1" style={{ marginTop: 16 }}>
          Do you want to edit messages before resending?
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="default">
          Cancel
        </Button>
        <Button onClick={onEditBeforeResend} color="primary">
          Edit Before Resend
        </Button>
        <Button onClick={onResend} color="primary" variant="contained">
          Resend Now
        </Button>
      </DialogActions>
    </Dialog>
  );
}