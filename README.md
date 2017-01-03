# GitHub Deploy
[![License](https://img.shields.io/github/license/JoeBiellik/ghd.svg)](LICENSE.md)
[![Release Version](https://img.shields.io/github/release/JoeBiellik/ghd.svg)](https://github.com/JoeBiellik/ghd/releases)
[![Dependency Status](https://img.shields.io/versioneye/d/user/projects/584b5112a662a5003f83c176.svg)](https://www.versioneye.com/user/projects/584b5112a662a5003f83c176)
[![Build Status](https://img.shields.io/travis/JoeBiellik/ghd.svg)](https://travis-ci.org/JoeBiellik/ghd)
[![Docker Build](https://img.shields.io/docker/automated/joebiellik/ghd.svg)](https://microbadger.com/images/joebiellik/ghd)
[![Docker Pulls](https://img.shields.io/docker/pulls/joebiellik/ghd.svg)](https://hub.docker.com/r/joebiellik/ghd/)

Simple cross platform service to run a command when triggered by a GitHub webhook. Built in C# using [.NET Native](https://msdn.microsoft.com/en-us/library/dn584397\(v=vs.110\).aspx) to support Windows, Linux and OSX.

## Running
You can [download a native binary of the latest release](https://github.com/JoeBiellik/ghd/releases/latest) or you can use the [Docker container](https://hub.docker.com/r/joebiellik/ghd/).

## Configuration
The service binds to `localhost:5000` by default, to bind to a different address use the `--server.urls` flag, e.g. `ghd --server.urls=http://0.0.0.0:80`.

All configuration options are stored in `settings.json`. Multiple repositories and branches can be watched by creating multiple profiles.

### SSH
#### Disabled
If you disable SSH then the deploy command will run locally under the user which launched the service.

```json
"SSH": {
  "Enabled": false
}
```

#### Password
```json
"SSH": {
  "Enabled": true,
  "Host": "192.168.0.1",
  "Port": 22,
  "Username": "root",
  "Password": "qwerty"
}
```

#### Public Key
```json
"SSH": {
  "Enabled": true,
  "Host": "192.168.0.1",
  "Port": 22,
  "Username": "root",
  "KeyFile": "/root/.ssh/id_rsa",
  "KeyPassword": null // or "qwerty"
}
```

### GitHub
The GitHub configuration is simply the repository and branch to accept webhooks from.

```json
"GitHub": {
  "Repository": "githubtraining/hellogitworld",
  "Branch": "master"
}
```

### Deploy
The deploy section specifies the command to run either locally or over SSH.
Both the `Command` and `Arguments` strings are interpolated with the [data from the GitHub webhook](https://developer.github.com/v3/activity/events/types/#pushevent), exposing the [full objects](Webhooks/GitHub.cs).

#### Linux example
```json
"Deploy": {
  "Command": "bash",
  "Arguments": "-c \"echo Deploying {Repository.FullName} [{HeadCommit.Timestamp}]: {HeadCommit.Message}\""
}
```

#### Windows example
```json
"Deploy": {
  "Command": "cmd",
  "Arguments": "/C echo Deploying {Repository.FullName} [{HeadCommit.Timestamp}]: {HeadCommit.Message}"
}
```

## Setup
Once the service is running, add a new webhook to your GitHub repository:
```
Payload URL: The service will accept any URL that resolves to it
Content type: application/json
Secret: Ignored
Event: Only the push event is currently used
```

## Building
If you have [.NET Core](https://www.microsoft.com/net/core) installed then just run `dotnet run -c Release` to start the service directly.
To build a portable native binary run `dotnet publish -c Release -r centos.7-x64` replacing the runtime flag with your target platform. The app will be built in `bin/Release/netcoreapp1.0/<runtime>/publish`. See [project.json](project.json) for supported runtimes.
Alternatively you can run and build the service under [Docker](https://www.docker.com/) with the provided [Compose file](docker-compose.yml).
