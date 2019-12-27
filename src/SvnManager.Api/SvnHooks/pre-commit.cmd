@echo off
::
:: Stops commits that have empty log messages.
::

setlocal

rem Subversion sends through the path to the repository and transaction id
set REPOS=%1
set TXN=%2

rem check for an empty log message
svnlook log %REPOS% -t %TXN% | findstr . > nul
if %errorlevel% gtr 0 (goto err) else exit 0

:err
echo. 1>&2
echo *** Commit message rejected *** 1>&2
echo Sorry, this commit was rejected because this repository requires 1>&2
echo you to post a commit message with every commit. 1>&2
echo. 1>&2
echo A meaningful commit message helps you remember what changed and keeps your team 1>&2
echo informed, saving you time when maintaining your code later. 1>&2
echo. 1>&2
echo A message should contain a short summary of your changes 1>&2
echo as well as an explanation to why you've done them or what problem they solve. 1>&2
echo. 1>&2
echo Please go back and commit again. 1>&2
exit 1