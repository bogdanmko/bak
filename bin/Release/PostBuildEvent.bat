@echo off
echo copy D:\MISC_PROJECTS\Bak\test_job.xml D:\MISC_PROJECTS\Bak\bin\Release\
copy D:\MISC_PROJECTS\Bak\test_job.xml D:\MISC_PROJECTS\Bak\bin\Release\
if errorlevel 1 goto CSharpReportError
goto CSharpEnd
:CSharpReportError
echo Project error: A tool returned an error code from the build event
exit 1
:CSharpEnd