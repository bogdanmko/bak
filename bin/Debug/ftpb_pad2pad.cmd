@echo user pad2pad_backup> ftp2p.dat
@echo 5alqza2dk>> ftp2p.dat
del %1*.sql
del %1*.zip
echo get joomla.sql %1joomla.sql>> ftp2p.dat
echo get pad2pad.zip %1pad2pad.zip>>ftp2p.dat
echo quit>> ftp2p.dat
ftp.exe -n -s:ftp2p.dat pad2pad.com
del ftp2p.dat
