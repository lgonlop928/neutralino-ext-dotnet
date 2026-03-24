// DotNetExtension
//
// Run DotNetExtension functions by sending dispatched event messages.
//
// (c)2023-2024 Harald Schneider - marketmix.com for the original Python Extension
// Adapted for .NET by lgonlop928

class DotNetExtension {
    constructor(debug=false) {
        this.version = '1.0.0';
        this.debug = debug;

        if(NL_MODE !== 'window') {
            window.addEventListener('beforeunload', function (e) {
                e.preventDefault();
                e.returnValue = '';
                DOTNET.stop();
                return '';
            });
        }
    }
    async run(f, p=null) {

        // Call a DotNetExtension function.

        let ext = 'extDotNet';
        let event = 'runDotNet';

        let data = {
            function: f,
            parameter: p
        }

        if(this.debug) {
            console.log(`EXT_DOTNET: Calling ${ext}.${event} : ` + JSON.stringify(data));
        }

        await Neutralino.extensions.dispatch(ext, event, data);
    }

    async stop() {

        // Stop and quit the DotNet extension and its parent app.
        // Use this if Neutralino runs in Cloud-Mode.

        let ext = 'extDotNet';
        let event = 'appClose';

        if(this.debug) {
            console.log(`EXT_DOTNET: Calling ${ext}.${event}`);
        }
        await Neutralino.extensions.dispatch(ext, event, "");
        await Neutralino.app.exit();
    }
}