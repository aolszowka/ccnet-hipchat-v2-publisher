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
  <room-name>NameOfTheRoomToPublishTo</room-name>
  <NotifyOnlyOnError>true</NotifyOnlyOnError>
</hipchat>
```

Note that the `NotifyOnlyOnError` is optional and by default is false. However it is recommended that you set this value to true if you have a lot of builds and don't want spam. When this value is set to true any successful build is not broadcast, **HOWEVER** "Fixed" builds are still broadcast (IE builds which were previously failing but are now passing) even with this setting off.

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

These return a HipChat.NET ```SendNotification``` object which closely mimics the v2 API for [Send room notification](https://www.hipchat.com/docs/apiv2/method/send_room_notification) the message format is defaulted to HTML and as per the documentation you can use basic HTML formatting. You are also passed a CruiseControl.NET ```IIntegrationResult``` which should allow you to extract anything of interest from the Current Integration.

### Known Issues
* The ``@Mentions`` does not work, as far as I am aware this is a limitation of the HipChat API.
* Only CruiseControl.NET 1.4.4SP1 is Supported Out of the Box; this is by design as mentioned several times.

### TODO
* Support Publishing to Multiple Rooms - (Forth coming)
* Support Customizing the Failure/Fixed/Success Messages (Maybe)
* Support Publishing to Custom HipChat URLs - (Maybe)
* Support Cross Reference to Convert Failure Users to HipChat Names - (Probably not)
* Support Multiple CruiseControl.NET Versions - (Look to forks for this, unless I need to support a new version of Cruise control.NET I am unlikely to do this) 
