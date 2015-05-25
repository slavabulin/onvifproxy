netsh interface ip set address name="lan" static 192.168.1.170 255.255.255.0
netsh interface ip add address name="lan" addr=192.168.0.170 mask=255.255.255.0 gateway=192.168.0.160 gwmetric=1
netsh interface ip set dns name="lan" source=static addr=8.8.8.8
copy config.xml! config.xml