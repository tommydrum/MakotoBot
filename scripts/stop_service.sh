#!/bin/bash

# stop dotnet application
systemctl stop kestrel-makoto.service
pkill dotnet # to be sure the service is dead.
exit 0 # gracefully exit. If service failed to stop, it's dead now.