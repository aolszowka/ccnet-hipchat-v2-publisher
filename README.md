# ccnet-hipchat-v2-publisher
A Simple CruiseControl.NET HipChat v2 Publisher.

Currently it will send a room notification for three states:

```
[FAILED] Quick-CI Build Log. Breakers: @aceo
[FIXED] Quick-CI
[SUCCESSFUL] Quick-CI
```

The ```[FAILED]``` state will link the Build Log to the Project URL specified in the CruiseControl.NET Project Configuration, and append an @ to any of the build failure users. However notifications do not work, see the Known Issues section for more.

### Requirements
* This publisher is built for CruiseControl.NET 1.4.4SP1. See the section Hacking on how to upgrade this.
* This publisher must be built against .NET 4.5+ due to the dependency on HipChat.NET

### How To Install
**NOTE** If you are using CruiseControl.NET 1.4.4SP1 (or any version of CruiseControl.NET that was not built or running against the .NET 4.5 Framework) see the Common Issues below for how to setup CruiseControl.NET to use the .NET 4 Runtime (which supports 4/4.5+)

* Grab the source and compile.
* Copy the binary over to the CruiseControl.NET installation folder.
* Modify your ccnet.config to add the following to the publishers section, restart CruiseControl.NET to pick up the configuration change.

```xml
<hipchat>
  <auth-token>OAuthTokenHere</auth-token>
  <room-names>NameOfTheRoomToPublishTo;MultipleRoomsAreSupported;DelimitedBySemiColon</room-names>
  <notify-only-on-error>true</notify-only-on-error>
</hipchat>
```

Note that the `notify-only-on-error` is optional and by default is ```false```. However it is recommended that you set this value to ```true``` if you have a lot of builds and don't want spam. When this value is set to ```true``` only failures are broadcast, additionally "Fixed" builds are also broadcast (IE builds which were previously failing but are now passing), this seemed to be the best compromise. If you don't like this behavior check out the hacking section for information on how to turn this off.

As mentioned above you can publish to multiple rooms, simply delimit the room name by semicolon (```;```). This also means that rooms that have a semicolon in them are not supported.

### Common Issues
Some versions of CruiseControl.NET are not built against the .NET 4.5 Framework (which uses the 4.0 runtime) because of this it cannot load any plug-ins that are build against Framework versions greater than 3.5 (the .NET 2.0 runtime). The fix to this is to modify the App.config of CruiseControl to advertise as supporting the .NET 4.0 runtime. This process is documented on MSDN in the [How to: Configure an App to Support .NET Framework 4 or 4.5](https://msdn.microsoft.com/en-us/library/jj152935%28v=vs.110%29.aspx).

The quick and dirty is to add the following to **ALL** of the CruiseControl.NET configuration files:
* ccnet.exe.config - The Interactive Console CruiseControl.NET Application
* ccservice.exe.config - The Service Application (99% of users are running CCNET via this service)
* CCValidator.exe.config - The CruiseControl.NET Configuration Validation Program (this file may not exist, create it)

```xml
<configuration>
  <startup>
    <supportedRuntime version="v4.0"/>
  </startup>
</configuration>
```

In the case of ccnet.exe/ccservice.exe there is already a startup tag, simply append the ```supportedRuntime``` tag within the ```startup``` section and relaunch the app/restart the service.

### Hacking
This project is licensed under the MIT License and you are encouraged to fork this project to suit your needs. I also happily take pull requests for functionality that does not require a bump in the version of CruiseControl.NET. Bonus points for including unit tests (don't forget to strip out any OAuth Keys!).

#### Upgrading Supported CruiseControl.NET Version
The most common change will probably be upgrading to a much more recent version of CruiseControl.NET. While I have not tested it and its been a long time since I've actively followed CruiseControl.NET's plugin architecture. It should, in theory, be as simple as pulling the following files from your CruiseControl.NET instance and updating the references in the project:

* NetReflector.dll
* ThoughtWorks.CruiseControl.Core.dll
* ThoughtWorks.CruiseControl.Remote.dll

#### Changing the Messages
The next common change will be changing the format of the messages that are printed. The creation of messages (as of this writing) are abstracted to the aptly named:

* ```IntegrationFailedMessage```
* ```IntegrationFixedMessage```
* ```IntegrationSuccessfulMessage```

These return a HipChat.NET ```SendNotification``` object which closely mimics the v2 API for [Send room notification](https://www.hipchat.com/docs/apiv2/method/send_room_notification) the message format is defaulted to HTML and as per the documentation you can use basic HTML formatting. You are also passed a CruiseControl.NET ```IIntegrationResult``` which should allow you to extract anything of interest from the Current Integration. A possible change might be to use a different color for fixed integrations to more quickly call them out from a successful one.

#### Notifying Only On Error
Currently when Notify Only On Error is ```true``` you only get notifications when an Intergration fails or is fixed (IE was previously failing). If you want to change this behavior take a look at ```EvaluateIntegration(IIntegrationResult, bool)``` this function is expected to return a ```Tuple<bool, SendNotification>``` where the first element dictates whether or not to post the second element to the room.

### Known Issues
Check [Issues/known-issue](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/labels/known-issue) for more.

* [``@Mentions`` do not work](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/issues/1)
* [Only CruiseControl.NET 1.4.4SP1 is Supported Out of the Box](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/issues/2)
* [Installing The Plugin Causes A System.BadImageFormatException](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/issues/3)

### TODO
Check the [Issues/enhacement](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/labels/enhancement) for more.

* [Support Publishing to Multiple Rooms](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/issues/4)
* [Support Customizing the Failure/Fixed/Success Messages](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/issues/5)
* [Support Publishing to Custom HipChat URLs](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/issues/6)
* [Support Cross Reference to Convert Failure Users to HipChat Names](https://github.com/aolszowka/ccnet-hipchat-v2-publisher/issues/7)

## Copyright & License
* ccnet-hipchat-v2-publisher Copyright 2015 Ace Olszowka [MIT License](LICENSE.txt)
* HipChat.NET Copyright 2014 Chris Kirby [MIT License](https://github.com/sirkirby/hipchat.net/blob/master/LICENSE.txt)
* CruiseControl.NET Copyright 2005 ThoughtWorks, Inc [ThoughtWorks Open Source Software License](https://raw.githubusercontent.com/ccnet/CruiseControl.NET/0ced9ffb9f651474dd09a38e756064c8ebd5e220/license.txt)
