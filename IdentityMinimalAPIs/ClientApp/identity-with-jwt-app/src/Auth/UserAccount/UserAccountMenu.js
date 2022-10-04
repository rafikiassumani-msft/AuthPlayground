import * as React from "react";
import PropTypes from "prop-types";
import Tabs from "@mui/material/Tabs";
import Tab from "@mui/material/Tab";
import Box from "@mui/material/Box";
import { Card, CardContent, Divider, Grid } from "@mui/material";
import AccountBoxIcon from "@mui/icons-material/AccountBox";
import LockIcon from "@mui/icons-material/Lock";
import PasswordIcon from "@mui/icons-material/Password";
import TtyIcon from "@mui/icons-material/Tty";
import UserInfoTab from "./UserInfoTab";
import ChangePasswordTab from "./ChangePasswordTab";
import PhoneNumberSettingsTab from "./PhoneNumberSettingsTab";
import TwoFactorAuthTab from "./TwoFactorAuthTab";
import { useAuth } from "../AuthProvider";

function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`vertical-tabpanel-${index}`}
      aria-labelledby={`vertical-tab-${index}`}
      {...other}
    >
      {value === index && <Box>{children}</Box>}
    </div>
  );
}

TabPanel.propTypes = {
  children: PropTypes.node,
  index: PropTypes.number.isRequired,
  value: PropTypes.number.isRequired,
};

function a11yProps(index) {
  return {
    id: `vertical-tab-${index}`,
    "aria-controls": `vertical-tabpanel-${index}`,
  };
}

export default function UserAccountMenu() {
  const [value, setValue] = React.useState(0);
  const { userData } = useAuth();

  const handleTabChange = (event, newValue) => {
    setValue(newValue);
  };

  return (
    <Box
      sx={{
        flexGrow: 1,
        bgcolor: "background.paper",
        display: "flex",
        height: 400,
      }}
    >
      <Grid container spacing={3}>
        <Grid item xs={4}>
          <Card>
            <CardContent>
              <Tabs
                orientation="vertical"
                variant="scrollable"
                value={value}
                onChange={handleTabChange}
                aria-label="Vertical icon tabs example"
              >
                <Tab
                  icon={<AccountBoxIcon />}
                  iconPosition="start"
                  label="Personal Info"
                  {...a11yProps(0)}
                  sx={{ justifyContent: "flex-start" }}
                />
                <Tab
                  icon={<LockIcon />}
                  iconPosition="start"
                  label="Two Factor Auth"
                  {...a11yProps(1)}
                  sx={{ justifyContent: "flex-start" }}
                />
                <Tab
                  icon={<PasswordIcon />}
                  iconPosition="start"
                  label="Change Password"
                  {...a11yProps(2)}
                  sx={{ justifyContent: "flex-start" }}
                />
                <Tab
                  icon={<TtyIcon />}
                  iconPosition="start"
                  label="Verify Phone Number"
                  {...a11yProps(3)}
                  sx={{ justifyContent: "flex-start" }}
                />
              </Tabs>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={8}>
          <TabPanel value={value} index={0}>
            {userData && <UserInfoTab />}
          </TabPanel>
          <TabPanel value={value} index={1}>
            <TwoFactorAuthTab />
          </TabPanel>
          <TabPanel value={value} index={2}>
            <ChangePasswordTab />
          </TabPanel>
          <TabPanel value={value} index={3}>
            <PhoneNumberSettingsTab />
          </TabPanel>
        </Grid>
      </Grid>
    </Box>
  );
}
