# GitHub Deploy
[![License](https://img.shields.io/github/license/JoeBiellik/ghd.svg)](LICENSE.md)
[![Release Version](https://img.shields.io/github/release/JoeBiellik/ghd.svg)](https://github.com/JoeBiellik/ghd/releases)

Simple cross platform service to run a command when triggered by a GitHub webhook. Built in C# using [.NET Native](https://msdn.microsoft.com/en-us/library/dn584397\(v=vs.110\).aspx) to support Windows, Linux and OSX.

## Configuration
All configuration options are stored in `settings.json`.

### SSH
#### Disabled example
If you disable SSH then the command will run locally under the user used to launch the service.

```json
"SSH": {
  "Enabled": false
}
```

#### Password example
```json
"SSH": {
  "Enabled": true,
  "Host": "192.168.0.1",
  "Port": 22,
  "Username": "root",
  "Password": "qwerty"
}
```

#### Public key example
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

## GitHub
The GitHub configuration is simply the repository and branch to accept webhooks from.

```json
"GitHub": {
  "Repository": "githubtraining/hellogitworld",
  "Branch": "master"
```

## GitHub
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
