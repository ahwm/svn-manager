# SVN Manager

[![Build status](https://ci.appveyor.com/api/projects/status/sth4yasy9nop9hsu?svg=true)](https://ci.appveyor.com/project/ahwm/svn-manager)

[![GitHub](https://img.shields.io/github/license/ahwm/svn-manager.svg)](https://github.com/ahwm/svn-manager/blob/master/LICENSE)
[![opened issues](https://img.shields.io/github/issues/ahwm/svn-manager.svg)](https://github.com/ahwm/svn-manager/issues)

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=U3LZULHW5ECLN&source=url)

## What it is
SVN Manager is a Windows Service licensed under the MIT license that allows you to create some automated tasks as well as open up a REST API endpoint (using NancyFx) for creating users and repositories.

It will also facilitate automated backups and (currently) upload those to Amazon S3. Over time this will be made more modular so other providers can be used.

## What it isn't
This service will not provide a web interface for browsing SVN (at least for now - this may be added later). It will let you see a list of existing repositories and allow you to create new ones.
