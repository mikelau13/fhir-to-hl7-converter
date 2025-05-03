// File: frontend/src/components/Layout.jsx
import React from 'react';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Container, 
  makeStyles, 
  Button,
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  IconButton,
  Divider
} from '@material-ui/core';
import { 
  Dashboard as DashboardIcon,
  Settings as SettingsIcon,
  Notifications as NotificationsIcon,
  Menu as MenuIcon
} from '@material-ui/icons';
import { Link as RouterLink } from 'react-router-dom';

const drawerWidth = 240;

const useStyles = makeStyles((theme) => ({
  root: {
    display: 'flex',
    minHeight: '100vh',
  },
  appBar: {
    zIndex: theme.zIndex.drawer + 1,
  },
  drawer: {
    width: drawerWidth,
    flexShrink: 0,
  },
  drawerPaper: {
    width: drawerWidth,
  },
  drawerContainer: {
    overflow: 'auto',
  },
  menuButton: {
    marginRight: theme.spacing(2),
    [theme.breakpoints.up('md')]: {
      display: 'none',
    },
  },
  title: {
    flexGrow: 1,
  },
  content: {
    flexGrow: 1,
    padding: theme.spacing(0),
    backgroundColor: theme.palette.background.default,
  },
  toolbar: theme.mixins.toolbar,
  navLink: {
    textDecoration: 'none',
    color: 'inherit',
  },
  userInfo: {
    marginLeft: theme.spacing(2),
  },
}));

const Layout = ({ children }) => {
  const classes = useStyles();
  const [mobileOpen, setMobileOpen] = React.useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const drawer = (
    <div>
      <div className={classes.toolbar} />
      <Divider />
      <List>
        <ListItem button component={RouterLink} to="/">
          <ListItemIcon>
            <DashboardIcon />
          </ListItemIcon>
          <ListItemText primary="Dashboard" />
        </ListItem>
        <ListItem button>
          <ListItemIcon>
            <NotificationsIcon />
          </ListItemIcon>
          <ListItemText primary="Notifications" />
        </ListItem>
        <ListItem button>
          <ListItemIcon>
            <SettingsIcon />
          </ListItemIcon>
          <ListItemText primary="Settings" />
        </ListItem>
      </List>
    </div>
  );

  return (
    <div className={classes.root}>
      <AppBar position="fixed" className={classes.appBar}>
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={handleDrawerToggle}
            className={classes.menuButton}
          >
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" className={classes.title}>
            FHIR to HL7 Integration System
          </Typography>
          <Typography variant="body2" className={classes.userInfo}>
            John Doe | Admin
          </Typography>
          <Button color="inherit">Logout</Button>
        </Toolbar>
      </AppBar>
      
      <Drawer
        className={classes.drawer}
        variant="permanent"
        classes={{
          paper: classes.drawerPaper,
        }}
      >
        {drawer}
      </Drawer>
      
      <main className={classes.content}>
        <div className={classes.toolbar} />
        <Container maxWidth="xl" style={{ padding: 0 }}>
          {children}
        </Container>
      </main>
    </div>
  );
};

export default Layout;
