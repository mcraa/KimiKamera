# Kimi-Kame-ra
> [Is it on or not?](https://www.youtube.com/watch?v=hI0Q7IPWjOk&t=13s) - *Kimi Räikönnen* 

A simple console application / background service written in `dotnet` for checking the state of your webcamera, and notifying you through the selected services. So people behind you don't need to disrupt you by asking *"Is it on or not?"*

## Available services
### [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr)
Mainly for debugging, but if you have a spare monitor or you just want to log the statuses this is the way to go.
Enable it by passing the `Hub URL` with the `-surl` command line parameter.
The message structure is matching the examples by [dotnet/AspNetCore.Docs](https://github.com/dotnet/AspNetCore.Docs/tree/main/aspnetcore/signalr/javascript-client/samples/3.x/SignalRChat) so just spinnig up that project you have a compatible hub with zero coding.

### [BlinkStick](https://www.blinkstick.com/)
The main goal of this project to tell others around you that your camera is on.
Enable the `BlinkStick` service with passing the number of leds through the `-bsl` command line parameter.

The service uses the [official library](https://github.com/arvydas/BlinkStickDotNet) from the manufacturer. The built dlls are in the `lib` folder

### What service do you need?
Pls add an issue or PR

## Command line parameters
| Handle | Type | Meaning | Example |
|----------|:------:|:---------:|---|
| `-i` | number | time in milliseconds between two queries for the camera availability, if not provided defaults to `5000` | `6000`
| `-bsl` | number | if provided creates BlinkStick service with the given amount of leds | `8`
| `-surl` | string | if provided creates SignalR connection to the provided url | `http://localhost:5000`

## Example usage

### On windows 
You can install the program as a service with [sc.exe](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/sc-create). Make sure you set it to `Run as Administrator`, querying the camera is not working as a normal user.
```
cd path/to/project
dotnet run -- -bsl 8 -i 2000
```
or
```
cd path/to/executable
KimiKamera.exe -bsl 8
```

### On Linux and Mac (not implemented yet)
```
cd path/to/executable
KimiKamera -bsl 8
```