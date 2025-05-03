// File: frontend/src/components/MessageList.tsx
import React from 'react';
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableContainer, 
  TableHead, 
  TableRow,
  Checkbox,
  Button,
  Typography,
  Chip,
  CircularProgress
} from '@material-ui/core';
import { useNavigate } from 'react-router-dom';
import { useMessages } from '../context/MessageContext';

// Define Message interface
interface Message {
  id: string;
  patientId: string;
  timestamp: string;
  status: string;
}

// Define Props interface
interface MessageListProps {
  selectedMessages: string[];
  onSelectionChange: (selected: string[]) => void;
  onBatchResend: () => void;
}

export default function MessageList({ 
  selectedMessages, 
  onSelectionChange, 
  onBatchResend 
}: MessageListProps) {
  const navigate = useNavigate();
  const { messages, loading, error, resendMessage } = useMessages();
  
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      // Select all messages
      onSelectionChange(messages.map(message => message.id));
    } else {
      // Deselect all
      onSelectionChange([]);
    }
  };
  
  const handleSelect = (id: string) => {
    if (selectedMessages.includes(id)) {
      // Remove from selection
      onSelectionChange(selectedMessages.filter(messageId => messageId !== id));
    } else {
      // Add to selection
      onSelectionChange([...selectedMessages, id]);
    }
  };
  
  const handleResend = async (id: string) => {
    await resendMessage(id);
  };
  
  const handleViewDetails = (id: string) => {
    navigate(`/messages/${id}`);
  };
  
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Sent':
        return 'primary';
      case 'Failed':
        return 'secondary';
      case 'Pending':
        return 'default';
      default:
        return 'default';
    }
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 20 }}>
        <CircularProgress />
      </div>
    );
  }

  if (error) {
    return (
      <Typography color="error">
        Error loading messages: {error}
      </Typography>
    );
  }

  if (messages.length === 0) {
    return (
      <Typography>
        No messages found matching the current filters.
      </Typography>
    );
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Typography variant="h6">Messages ({messages.length})</Typography>
        <Button 
          variant="contained" 
          color="primary" 
          disabled={selectedMessages.length === 0}
          onClick={onBatchResend}
        >
          Batch Resend ({selectedMessages.length} selected)
        </Button>
      </div>
      
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell padding="checkbox">
                <Checkbox
                  indeterminate={selectedMessages.length > 0 && selectedMessages.length < messages.length}
                  checked={messages.length > 0 && selectedMessages.length === messages.length}
                  onChange={handleSelectAll}
                />
              </TableCell>
              <TableCell>Message ID</TableCell>
              <TableCell>Patient ID</TableCell>
              <TableCell>Timestamp</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {messages.map((message) => (
              <TableRow key={message.id}>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={selectedMessages.includes(message.id)}
                    onChange={() => handleSelect(message.id)}
                  />
                </TableCell>
                <TableCell>{message.id}</TableCell>
                <TableCell>{message.patientId}</TableCell>
                <TableCell>{new Date(message.timestamp).toLocaleString()}</TableCell>
                <TableCell>
                  <Chip 
                    label={message.status} 
                    color={getStatusColor(message.status)} 
                    size="small" 
                  />
                </TableCell>
                <TableCell>
                  <div style={{ display: 'flex', gap: 8 }}>
                    <Button 
                      variant="outlined" 
                      size="small"
                      onClick={() => handleViewDetails(message.id)}
                    >
                      View
                    </Button>
                    <Button 
                      variant="outlined" 
                      size="small"
                      color="primary"
                      onClick={() => handleResend(message.id)}
                      disabled={message.status === 'Pending'}
                    >
                      Resend
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </div>
  );
}