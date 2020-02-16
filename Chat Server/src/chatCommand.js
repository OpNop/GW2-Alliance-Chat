module.exports = class chatCommand {
    
    commands = {};
    unknownCommand = "Unknown Command";

    constructor(commands) {
        commands.forEach(command => {
            // Check if command has aliases 
            if(Array.isArray(command.names)){
                command.names.forEach(alias => {
                    this.addCommand(alias, command.func, (command.permission || 'user'));
                })
            } else {
                this.addCommand(command.names, command.func, (command.permission || 'user'));
            }
        });
    }

    addCommand(command, callback, permission) {
        this.commands[command] = {runner: callback, permission: permission}
    }

    run(socket, command, args) {
        if(typeof this.commands[command].runner === 'function'){
            this.commands[command].runner(socket, args);
        } else{
            throw this.unknownCommand;
        }
    }

    get allCommands() {
        return Object.keys(this.commands).map(key => `/${key}`).join(', ');
    }
}
