#!/bin/sh

cat >/etc/motd <<EOL
                            .__                                    
_____ _____ ___________ | __ | ____ _____ ____ _____ ______
\ __ \ / \ _ / __ \ _ __ \ | / ___ \\ __ \ / \\ __ \ / ___ /
 / __ \ | YY \ ___ / | | \ / \ \ ___ / __ \ | | \ / __ \ _ \ ___ \
(____ / __ | _ | / \ ___> __ | | __ | \ ___> ____ / ___ | (____ / ____>
     \ / \ / \ / \ / \ / \ / \ / \ / 
A P P   S E R V I C E   O N   L I N U X
Documentation: http://aka.ms/webapp-linux
EOL
cat /etc/motd

service ssh start

# Get environment variables to show up in SSH session
eval $(printenv | awk -F= '{print "export " $1"="$2 }' >> /etc/profile)

exec dotnet /app/quickstartcore.dll
