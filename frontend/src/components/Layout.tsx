// File: frontend/src/components/Layout.tsx
import React, { ReactNode } from 'react';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Container, 
  makeStyles 
} from '@material-ui/core';

const useStyles = makeStyles((theme) => ({
  root: {
    display: 'flex',
    flexDirection: 'column',
    minHeight: '100vh',
  },
  content: {
    flexGrow: 1,
    padding: theme.spacing(3),
    marginTop: 64,
  },
  title: {
    flexGrow: 1,
  },
}));

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const classes = useStyles();

  return (
    <div className={classes.root}>
      <AppBar position="fixed">
        <Toolbar>
          <Typography variant="h6" className={classes.title}>
            FHIR to HL7 Integration System
          </Typography>
          {/* TODO: Add user info or navigation links */}
        </Toolbar>
      </AppBar>
      <main className={classes.content}>
        <Container maxWidth="xl">
          {children || <div />}
        </Container>
      </main>
    </div>
  );
}