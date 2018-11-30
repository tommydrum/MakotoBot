#!/bin/bash
cd /home/ec2-user/

# use systemd to start and monitor dotnet application
systemctl enable kestrel-makoto.service
systemctl start kestrel-makoto.service