# Intro #
This console program is used with OpenVPN to authenticate Active Directory users.

# Usage #
1. Download the zip file [here](https://github.com/markhuynh/ADAuthForOpenVPN/releases/download/1.0/ADAuthForOpenVPN.zip).
2. Place ADAuthForOpenVPN.exe and ADAuthForOpenVPN.ini in your OpenVPN config folder.
3. Update ADAuthForOpenVPN.ini with the right domain and, optionally, the DN and Groups.
4. Update your server ovpn file and add two lines (without quotes):
   "script-security 2"
   "auth-user-pass-verify ADAuthForOpenVPN.exe via-file"
5. Errors are logged in Windows Application Event Log.

# Additional steps when running OpenVPN as a service #
1. The OpenVPN service needs to be run with an administrator account instead of the default "Local System"
