module.exports = class chatCommand {
    constructor() {}
    commands = {};
    unknownCommand = "Unknown Command";

    addCommand(command, callback) {
        this.commands[command] = callback
        //this.commands.push({[command]: callback});
    }

    run(socket, command, args) {
        if(typeof this.commands[command] === 'function'){
            this.commands[command](socket, args);
        } else{
            throw this.unknownCommand;
        }
    }

    get allCommands() {
        return Object.keys(this.commands).map(key => `/${key}`).join(', ');
    }
}
