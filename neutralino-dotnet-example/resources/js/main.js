// This is just a sample app. You can structure your Neutralinojs app code as you wish.
// This example app is written with vanilla JavaScript and HTML.
// Feel free to use any frontend framework you like :)
// See more details: https://neutralino.js.org/docs/how-to/use-a-frontend-library

/*
    Function to display information about the Neutralino app.
    This function updates the content of the 'info' element in the HTML
    with details regarding the running Neutralino application, including
    its ID, port, operating system, and version information.
*/
function showInfo() {
    document.getElementById('info').innerHTML = `
        ${NL_APPID} is running on port ${NL_PORT} inside ${NL_OS}
        <br/><br/>
        <span>server: v${NL_VERSION} . client: v${NL_CVERSION}</span>
        `;
}

/*
    Function to open the official Neutralino documentation in the default web browser.
*/
function openDocs() {
    Neutralino.os.open("https://neutralino.js.org/docs");
}

/*
    Function to open a tutorial video on Neutralino's official YouTube channel in the default web browser.
*/
function openTutorial() {
    Neutralino.os.open("https://www.youtube.com/c/CodeZri");
}

/*
    Function to handle the window close event by gracefully exiting the Neutralino application.
*/
function onWindowClose() {
    Neutralino.app.exit();
}

async function onReceivedText(e)
{
    document.getElementById("text").innerHTML = "<h1>"+e.detail+"</h1>";
}

// Initialize Neutralino
Neutralino.init();

// Register event listeners
Neutralino.events.on("windowClose", onWindowClose);
Neutralino.events.on("receivedText", onReceivedText);

// Display app information
showInfo();

const DOTNET = new DotNetExtension(true);