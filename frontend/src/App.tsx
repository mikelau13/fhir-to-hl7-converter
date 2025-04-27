// File: frontend/src/App.tsx
import React from 'react';
import { Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@material-ui/core/styles';
import CssBaseline from '@material-ui/core/CssBaseline';
import Dashboard from './pages/Dashboard';
import MessageDetails from './pages/MessageDetails';
import ClinicManagement from './pages/ClinicManagement';
import Layout from './components/Layout';
import { ClinicProvider } from './context/ClinicContext';
import { MessageProvider } from './context/MessageContext';

const theme = createTheme({
  // TODO: Customize theme
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <ClinicProvider>
        <MessageProvider>
          <Layout>
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/messages/:id" element={<MessageDetails />} />
              <Route path="/clinics" element={<ClinicManagement />} />
            </Routes>
          </Layout>
        </MessageProvider>
      </ClinicProvider>
    </ThemeProvider>
  );
}

export default App;