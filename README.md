<p align="center">
<img src="https://github.com/user-attachments/assets/c564d550-4206-4d50-91ce-de5c5a399586" style="margin-left:auto;margin-right:auto">
</p>


# neutralino-ext-dotnet
**A .NET Extension for NeutralinoJS.**

This extension has been adapted from the [neutralino-ext-python](https://github.com/hschneider/neutralino-ext-python) extension by [hschneider](https://github.com/hschneider). All the hard work was done by them, I merely translated the Python code to C# and changed how it works a bit to suit my tastes.

This README is also adapted from that repository.

## What is this?
This extension adds a .NET backend written in C# to Neutralino with the following features:
- Requires only a few lines of code on both ends.
- Run .NET functions from Neutralino.
- Run Neutralino functions from .NET
- All communication between Neutralino and .NET runs asynchronously.
- Terminates the .NET backend when the Neutralino app quits.

## Run the demo
![video-neutralino-net](https://github.com/user-attachments/assets/fb184cac-183d-4bc6-b763-ecfdcfc8ebc4)

The demo opens a Neutralino app. The backend automatically start sending the current time to the UI. When you click on the button, a "Hello World" is sent to the backend and then sent back to the UI.

To run the demo you'll need a Linux OS with the [neu CLI](https://neutralino.js.org/docs/cli/neu-cli/) installed. Simply clone this repository and go into the `neutralino-dotnet-example` folder, then run these commands:
```commandline
neu update
neu run
```

## Integrate into your own project
If you want to add the ```neutralino-dotnet-ext``` to an existing project or to the sample NeutralinoJS project created with ```neu create myapp```, just follow these steps:
- We need to do a few modifications to the ```neutralino.config.json``` file.
- First, add this line under ```"enableServer"``` and ```"enableNativeAPI"``` to enable extensions in the project:
```json
"enableExtensions": true,
```
- Next, modify the ```"nativeAllowList"``` so that it includes this whitelist:
```json
  "nativeAllowList": [
    "app.*",
    "os.*",
    "window.*",
    "events.*",
    "extensions.*",
    "debug.log"
  ],
```
- And last, add the ```"extensions"``` section at the end of the file:
```json
  "extensions": [
    {
      "id": "extDotNet",
      "commandLinux": "/route/to/your/dotnet/linux/binary",
      "commandWindows": "/route/to/your/dotnet/windows/binary",
      "commandDarwin": "/route/to/your/dotnet/macos/binary"
    }
  ]
```

<br/>

- In your Neutralino project, copy the ```/resources/js/dotnet-extension.js``` from the sample project to you ```/resources/js``` folder.
- Add `<script src="js/dotnet-extension.js"></script>` to your ```index.html```
- Add `const DOTNET = new DotNetExtension(true)` to your ``main.js``

<br/>

- In your .NET project, add the ```NeutralinoExtension.cs``` library.
- Create this global variable:
```csharp
static NeutralinoExt neutralino;
```
- And then put this code in the ```main()``` method:
```csharp
neutralino = new NeutralinoExt(true);
neutralino.RunForever();
```

## Sending messages from the UI to the .NET backend
* In an event in your ```index.html``` or in a function in your ```main.js``` file, just call:
```javascript
DOTNET.run('eventName', 'data')
```
Where ```eventName``` is the event you want to trigger in the backend and the ```data``` is the data you want to send.
* In your .NET project, you need to add the event handler before the call to ```neutralino.RunForever()``` like this:
```csharp
neutralino.AddEvent("eventName", MyFunction);
```
* And finally, write the function that does stuff:
```csharp
private static void MyFunction(string s)
{
    // Do stuff, for example:
    neutralino.DebugLog("Message Received: " + s);
}
```

## Sending messages from the .NET backend to the UI
* Anywhere in your .NET code, just call:
```csharp
neutralino.SendMessage("eventName", "data");
```
Where, again, ```eventName``` is the event you want to trigger in the UI and the ```data``` is the data you want to send.
* Then, in your JavaScript code, add the event handler after the ```Neutralino.init()``` line:
```javascript
Neutralino.events.on("eventName", myFunction);
```
* And write a function that does stuff:
```javascript
async function myFunction(e)
{
    // Do stuff, for example:
    document.getElementById("text").innerHTML = "<h1>"+e.detail+"</h1>";
}
```

## RunForever() vs Run()
The main difference between these two is that ```RunForever()``` enters an endless loop to wait for events while ```Run()``` does not. 
* If you want your app to be completely event-driven from the UI, use ```RunForever()``` and it will respond to events until the UI is closed.
* On the contrary, if you want your app to be backend-driven use ```Run()``` so you can continue to do stuff. You can still respond to events from your UI but if your backend reaches the end of the code it will close and it won't respond to events anymore. You'll probably want to close the UI before that happens by running ```Neutralino.app.exit()``` in your JavaScript side.
      
