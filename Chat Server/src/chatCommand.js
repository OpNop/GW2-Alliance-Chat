module.exports = class chatCommand {
    
    commands = {};

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

    async run(user, command, args) {
        if(command in this.commands && typeof this.commands[command].runner === 'function'){
            await this.commands[command].runner(user, args);
        } else{
            user.sendSystemMessage(`Command "${command}" not found.`);
        }
    }

    get allCommands() {
        return Object.keys(this.commands).map(key => `/${key}`).join(', ');
    }
}
