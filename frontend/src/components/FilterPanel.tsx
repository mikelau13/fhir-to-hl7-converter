// File: frontend/src/components/FilterPanel.tsx
import React from 'react';
import { 
  Grid, 
  TextField, 
  MenuItem, 
  Button, 
  Typography 
} from '@material-ui/core';

// Define interface for filter parameters
interface FilterParams {
  patientId: string;
  status: string;
  fromDate?: string;
  toDate?: string;
}

// Define props interface
interface FilterPanelProps {
  filters: FilterParams;
  onFilterChange: (filters: FilterParams) => void;
}

export default function FilterPanel({ filters, onFilterChange }: FilterPanelProps) {
  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;
    onFilterChange({ ...filters, [name]: value });
  };
  
  const handleClear = () => {
    onFilterChange({
      patientId: '',
      status: ''
    });
  };
  
  const handleApply = () => {
    // TODO: Trigger API call with current filters
  };

  return (
    <div style={{ marginBottom: 24 }}>
      <Typography variant="h6" style={{ marginBottom: 16 }}>Filters</Typography>
      
      <Grid container spacing={2}>
        <Grid item xs={12} md={3}>
          <TextField
            name="patientId"
            label="Patient ID"
            value={filters.patientId}
            onChange={handleChange}
            fullWidth
          />
        </Grid>
        <Grid item xs={12} md={3}>
          <TextField
            name="status"
            label="Status"
            value={filters.status}
            onChange={handleChange}
            select
            fullWidth
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Pending">Pending</MenuItem>
            <MenuItem value="Sent">Sent</MenuItem>
            <MenuItem value="Failed">Failed</MenuItem>
          </TextField>
        </Grid>
        <Grid item xs={12} md={3}>
          <TextField
            name="fromDate"
            label="From Date"
            type="date"
            value={filters.fromDate || ''}
            onChange={handleChange}
            InputLabelProps={{
              shrink: true,
            }}
            fullWidth
          />
        </Grid>
        <Grid item xs={12} md={3}>
          <TextField
            name="toDate"
            label="To Date"
            type="date"
            value={filters.toDate || ''}
            onChange={handleChange}
            InputLabelProps={{
              shrink: true,
            }}
            fullWidth
          />
        </Grid>
        <Grid item xs={12}>
          <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
            <Button 
              variant="outlined" 
              onClick={handleClear}
            >
              Clear Filters
            </Button>
            <Button 
              variant="contained" 
              color="primary" 
              onClick={handleApply}
            >
              Apply Filters
            </Button>
          </div>
        </Grid>
      </Grid>
    </div>
  );
}