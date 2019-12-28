# SVN Manager

[![Build status](https://ci.appveyor.com/api/projects/status/sth4yasy9nop9hsu?svg=true)](https://ci.appveyor.com/project/ahwm/svn-manager)

## What it is
SVN Manager is a Windows Service that allows you to create some automated tasks as well as open up a REST API endpoint (using NancyFx) for creating users and repositories.

It will also facilitate automated backups and (currently) upload those to Amazon S3. Over time this will be made more modular so other providers can be used.

## What it isn't
This service will not provide a web interface for browsing SVN (at least for now - this may be added later). It will let you see a list of existing repositories and allow you to create new ones.

### Note
This was originally developed as an internal project to give us some automation using VisualSVN Server. There are a lot of references to VisualSVN-specific things such as VisualSVN-SvnAuthz.ini and calls to VisualSVN's PowerShell cmdlets. Over time these will be replaced with native SVN commands.
