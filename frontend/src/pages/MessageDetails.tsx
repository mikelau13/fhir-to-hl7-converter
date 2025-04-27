// frontend/src/pages/MessageDetails.tsx
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Paper, 
  Typography, 
  Button, 
  Grid, 
  TextField,
  Divider 
} from '@material-ui/core';
import { useMessages } from '../context/MessageContext';
import type { Message } from '../types';

// Define the params type for useParams
type MessageDetailsParams = {
  id: string;
};

export default function MessageDetails() {
  const { id } = useParams<MessageDetailsParams>();
  const navigate = useNavigate();
  const { getMessage, resendMessage, updateMessageContent } = useMessages();
  const [message, setMessage] = useState<Message | null>(null);
  const [editedContent, setEditedContent] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    const fetchMessage = async () => {
      if (!id) return;
      
      setIsLoading(true);
      try {
        const data = await getMessage(id);
        setMessage(data);
        setEditedContent(data.hl7Content);
        setError(null);
      } catch (err) {
        setError('Failed to load message details');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchMessage();
  }, [id, getMessage]);
  
  const handleEdit = () => {
    setIsEditing(true);
  };
  
  const handleCancel = () => {
    setIsEditing(false);
    if (message) {
      setEditedContent(message.hl7Content);
    }
  };
  
  const handleSave = async () => {
    if (!id) return;
    
    try {
      await updateMessageContent(id, editedContent);
      
      // Update local state
      if (message) {
        setMessage({
          ...message,
          hl7Content: editedContent
        });
      }
      setIsEditing(false);
    } catch (err) {
      setError('Failed to save changes');
      console.error(err);
    }
  };
  
  const handleResend = async () => {
    if (!id) return;
    
    try {
      await resendMessage(id, { editBeforeResend: false });
      // Show success message or update UI
    } catch (err) {
      setError('Failed to resend message');
      console.error(err);
    }
  };
  
  const handleContentChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setEditedContent(event.target.value);
  };
  
  if (isLoading) {
    return <Typography>Loading message details...</Typography>;
  }
  
  if (error) {
    return <Typography color="error">{error}</Typography>;
  }
  
  if (!message) {
    return <Typography>Message not found</Typography>;
  }

  return (
    <Paper style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Typography variant="h5">Message Details: {message.id}</Typography>
        <Button 
          variant="outlined" 
          onClick={() => navigate(-1)}
        >
          Back
        </Button>
      </div>
      
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Typography variant="subtitle1">Patient ID: {message.patientId}</Typography>
        </Grid>
        <Grid item xs={12} md={6}>
          <Typography variant="subtitle1">Status: {message.status}</Typography>
        </Grid>
      </Grid>
      
      <Divider style={{ margin: '16px 0' }} />
      
      <Typography variant="h6" style={{ marginBottom: 8 }}>HL7 Content</Typography>
      
      <TextField
        multiline
        rows={10}
        variant="outlined"
        fullWidth
        value={editedContent}
        onChange={handleContentChange}
        disabled={!isEditing}
        style={{ fontFamily: 'monospace' }}
      />
      
      <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 16 }}>
        {isEditing ? (
          <>
            <Button variant="outlined" onClick={handleCancel}>Cancel</Button>
            <Button variant="contained" color="primary" onClick={handleSave}>Save</Button>
          </>
        ) : (
          <>
            <Button variant="outlined" onClick={handleEdit}>Edit</Button>
            <Button variant="contained" color="primary" onClick={handleResend}>Resend</Button>
          </>
        )}
      </div>
    </Paper>
  );
}