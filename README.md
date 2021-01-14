# HsUpdateChecker
## Usage example:
### Subscribe to my telegram channel: https://t.me/HsUpdates
### Linux:
0. Install dotnet-runtime-3.1: https://docs.microsoft.com/en-us/dotnet/core/install/linux
1. Download latest release [here](https://github.com/DeNcHiK3713/HsUpdateChecker/releases/latest/download/HsUpdateChecker.zip "here").
2. Unzip it somewhere. (For example is /home/user/HsUpdateChecker/)
3. Edit settings in appsettings.json.
4. Create new service:

Run command: 
```bash
sudo nano /etc/systemd/system/HsUpdateChecker.service
```

then paste following into nano:
```bash
[Unit]
Description=HsUpdateChecker service
Wants=network-online.target
After=network-online.target systemd-networkd-wait-online.service

[Service]
Type=exec
WorkingDirectory=/home/user/HsUpdateChecker
ExecStart=/usr/bin/dotnet /home/user/HsUpdateChecker/HsUpdateChecker.dll

[Install]
WantedBy=multi-user.target
```
then press ctrl+s, then ctrl+x.

5. Create new timer:

Run command:
```bash
sudo nano /etc/systemd/system/HsUpdateChecker.timer
```

then paste folowing into nano:
```bash
[Unit]
Description=Execute HsUpdateChecker every 15 minutes

[Timer]
OnCalendar=*:0/15
Unit=HsUpdateChecker.service

[Install]
WantedBy=multi-user.target
```
then press ctrl+s, then ctrl+x.

6. Enable timer by tuping following command:
```bash
sudo systemctl enable HsUpdateChecker.timer
```
### Windows:
1. Download latest release [here](https://github.com/DeNcHiK3713/HsUpdateChecker/releases/latest/download/HsUpdateChecker.zip "here").
2. Unzip somewhere.
3. Edit settings in appsettings.json.
4. Create new task for task scheduler and add "on a schedule" trigger.
