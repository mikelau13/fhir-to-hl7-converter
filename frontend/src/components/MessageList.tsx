// File: frontend/src/components/MessageList.jsx
import React, { useState, useEffect } from 'react';
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableContainer, 
  TableHead, 
  TableRow,
  Paper,
  Checkbox,
  Button,
  Typography,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Box,
  Chip,
  CircularProgress
} from '@material-ui/core';
import { formatDistanceToNow } from 'date-fns';
import { makeStyles } from '@material-ui/core/styles';
import api from '../services/api';

const useStyles = makeStyles((theme) => ({
  root: {
    width: '100%',
  },
  title: {
    marginBottom: theme.spacing(2),
  },
  filterBox: {
    padding: theme.spacing(2),
    marginBottom: theme.spacing(2),
    backgroundColor: theme.palette.background.default,
  },
  filterForm: {
    display: 'flex',
    flexWrap: 'wrap',
    gap: theme.spacing(2),
  },
  filterField: {
    minWidth: 200,
  },
  tableContainer: {
    marginTop: theme.spacing(2),
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
  batchResendButton: {
    marginTop: theme.spacing(2),
  },
  loadingContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: 400,
  },
}));

const MessageList = ({ onOpenBatchResend, onViewDetails }) => {
  const classes = useStyles();
  const [messages, setMessages] = useState([]);
  const [selectedMessages, setSelectedMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState({
    patientId: '',
    status: '',
    fromDate: '',
    toDate: '',
  });

  // Fetch messages on component mount and when filters change
  useEffect(() => {
    fetchMessages();
  }, []);

  const fetchMessages = async () => {
    setLoading(true);
    try {
      // In a real implementation, this would pass the filters to the API
      const response = await api.getMessages(filters);
      setMessages(response.data);
    } catch (error) {
      console.error('Error fetching messages:', error);
      // In a real implementation, show an error notification
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (event) => {
    const { name, value } = event.target;
    setFilters({ ...filters, [name]: value });
  };

  const handleApplyFilters = () => {
    fetchMessages();
  };

  const handleClearFilters = () => {
    setFilters({
      patientId: '',
      status: '',
      fromDate: '',
      toDate: '',
    });
  };

  const handleSelectAll = (event) => {
    if (event.target.checked) {
      const allIds = messages.map(message => message.messageId);
      setSelectedMessages(allIds);
    } else {
      setSelectedMessages([]);
    }
  };

  const handleSelect = (messageId) => {
    if (selectedMessages.includes(messageId)) {
      setSelectedMessages(selectedMessages.filter(id => id !== messageId));
    } else {
      setSelectedMessages([...selectedMessages, messageId]);
    }
  };

  const handleResend = async (messageId) => {
    try {
      await api.resendMessage(messageId);
      fetchMessages(); // Refresh the list after resend
    } catch (error) {
      console.error('Error resending message:', error);
      // In a real implementation, show an error notification
    }
  };

  const handleBatchResend = () => {
    if (selectedMessages.length > 0) {
      onOpenBatchResend(selectedMessages);
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

    return <Chip label={status} className={className} size="small" />;
  };

  const formatTimestamp = (timestamp) => {
    try {
      return formatDistanceToNow(new Date(timestamp), { addSuffix: true });
    } catch (error) {
      return timestamp;
    }
  };

  if (loading && messages.length === 0) {
    return (
      <div className={classes.loadingContainer}>
        <CircularProgress />
      </div>
    );
  }

  return (
    <div className={classes.root}>
      <Typography variant="h5" className={classes.title}>
        Messages
      </Typography>
      
      <Paper className={classes.filterBox}>
        <Typography variant="h6" gutterBottom>
          Filters
        </Typography>
        <div className={classes.filterForm}>
          <TextField
            className={classes.filterField}
            label="Patient ID"
            name="patientId"
            value={filters.patientId}
            onChange={handleFilterChange}
          />
          <FormControl className={classes.filterField}>
            <InputLabel>Status</InputLabel>
            <Select
              name="status"
              value={filters.status}
              onChange={handleFilterChange}
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Pending">Pending</MenuItem>
              <MenuItem value="Sent">Sent</MenuItem>
              <MenuItem value="Failed">Failed</MenuItem>
            </Select>
          </FormControl>
          <TextField
            className={classes.filterField}
            label="From Date"
            name="fromDate"
            type="date"
            value={filters.fromDate}
            onChange={handleFilterChange}
            InputLabelProps={{
              shrink: true,
            }}
          />
          <TextField
            className={classes.filterField}
            label="To Date"
            name="toDate"
            type="date"
            value={filters.toDate}
            onChange={handleFilterChange}
            InputLabelProps={{
              shrink: true,
            }}
          />
          <Box>
            <Button
              variant="contained"
              color="primary"
              onClick={handleApplyFilters}
              style={{ marginRight: 8 }}
            >
              Apply Filters
            </Button>
            <Button
              variant="outlined"
              onClick={handleClearFilters}
            >
              Clear
            </Button>
          </Box>
        </div>
      </Paper>

      <TableContainer component={Paper} className={classes.tableContainer}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell padding="checkbox">
                <Checkbox
                  onChange={handleSelectAll}
                  checked={selectedMessages.length > 0 && selectedMessages.length === messages.length}
                  indeterminate={selectedMessages.length > 0 && selectedMessages.length < messages.length}
                />
              </TableCell>
              <TableCell>Message ID</TableCell>
              <TableCell>Patient ID</TableCell>
              <TableCell>Message Type</TableCell>
              <TableCell>Clinic</TableCell>
              <TableCell>Created</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {messages.map((message) => (
              <TableRow key={message.messageId}>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={selectedMessages.includes(message.messageId)}
                    onChange={() => handleSelect(message.messageId)}
                  />
                </TableCell>
                <TableCell>{message.messageId}</TableCell>
                <TableCell>{message.patientId}</TableCell>
                <TableCell>{message.messageType}</TableCell>
                <TableCell>{message.clinicName}</TableCell>
                <TableCell>{formatTimestamp(message.createdAt)}</TableCell>
                <TableCell>{renderStatus(message.status)}</TableCell>
                <TableCell>
                  <Button
                    variant="outlined"
                    size="small"
                    onClick={() => onViewDetails(message.messageId)}
                    style={{ marginRight: 8 }}
                  >
                    View
                  </Button>
                  <Button
                    variant="contained"
                    color="primary"
                    size="small"
                    onClick={() => handleResend(message.messageId)}
                    disabled={message.status === 'Pending'}
                  >
                    Resend
                  </Button>
                </TableCell>
              </TableRow>
            ))}
            {messages.length === 0 && (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Typography variant="body1" style={{ padding: 20 }}>
                    No messages found
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {selectedMessages.length > 0 && (
        <Button
          variant="contained"
          color="primary"
          className={classes.batchResendButton}
          onClick={handleBatchResend}
        >
          Batch Resend ({selectedMessages.length} selected)
        </Button>
      )}
    </div>
  );
};

export default MessageList;
