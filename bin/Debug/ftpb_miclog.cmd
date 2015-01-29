@echo user miclog_backup> ftp.dat
@echo Atao91a7fg>> ftp.dat
del %1*.sql
del %1*.zip
echo get grasshopper.zip %1grasshopper.zip>> ftp.dat
echo get oneloc.zip %1oneloc.zip>>ftp.dat
echo get machine-shop.zip %1machine-shop.zip>>ftp.dat
echo get miclog.zip %1miclog.zip>>ftp.dat
echo get miclog_oe.sql %1miclog_oe.sql>>ftp.dat
echo get miclog_pdb.sql %1miclog_pdb.sql>>ftp.dat
echo get miclog_software.sql %1miclog_software.sql>>ftp.dat
echo get miclog_wtd.sql %1miclog_wtd.sql>>ftp.dat
echo get quirkle.zip %1quirkle.zip>>ftp.dat
echo get rule110.zip %1rule110.zip>>ftp.dat
echo quit>> ftp.dat
ftp.exe -n -s:ftp.dat miclog.com
del ftp.dat
