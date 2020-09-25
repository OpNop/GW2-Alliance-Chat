String.prototype.firstWord = function () {
    return this.replace(/\s.*/, '')
}

const net = require('net');
const TINYPacket = require('./src/packets/Packet.js');
const packets = require("./src/packets/Packet").packets;
const connectPacket = require("./src/packets/Connect.js");
const messagePacket = require("./src/packets/Message.js");
const enterPacket = require("./src/packets/Enter.js");
const leavePacket = require("./src/packets/Leave.js");
const systemPacket = require("./src/packets/System.js");
const updatePacket = require("./src/packets/Update.js");

const config = require('./config.json');
const tcolors = require('./src/tinyColor.js').colors;
const tinyPrompt = require('serverline');
const chatCommand = require('./src/chatCommand.js');
const chatUser = require('./src/user.js');



tinyPrompt.init();
tinyPrompt.setCompletion(['help', 'list', 'say', 'announce', 'inspect']);
tinyPrompt.setPrompt('TINY> ');
tinyPrompt.on('line', line => {
    data = parseCommad(line);
    switch (data.command) {
        case 'help':
            console.log("=== Console Commands ===")
            console.log("HELP\t\t Shows this message.");
            console.log("LIST\t\t List the current users and map.");
            console.log("SAY\t\t Broadcast a message to all clients as \"Server\"");
            console.log("ANNOUNCE\t Broadcast an announcement to all clients");
            console.log("INSPECT\t\t Show the mumble data on a user");
            break;
        case 'list':
            (async () => {
                console.log(await listUsers());
            })();
            break;
        case 'say':
            broadcast("Server", data.args);
            break;
        case 'announce':
            break;
        case 'inspect':
            let character = data.args;
            if (!character) {
                console.log(tcolors.fg.Red, "Missing Argument: Character Name", tcolors.Reset);
            }
            let user = clients.find((client => {
                return client.mumbleData.name === character
            }));
            if (!user) {
                console.log(tcolors.fg.Red, "Character not found", tcolors.Reset);
            } else {
                console.dir(user.mumbleData);
            }
            break;
        default:

            break;
    }
})

// Keep track of the chat clients
let clients = [];

//#region Chat Commands (dear lord I want to put these someplace else)
const chatCommands = [{
        names: 'beckon',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} beckons to ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} beckons.`);
        }
    },
    {
        names: 'bow',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} bows for ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} bows.`);
        }
    },
    {
        names: 'cheer',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} cheers for ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} cheers.`);
        }
    },
    {
        names: 'cower',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} cowers in fear from ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} cowers.`);
        }
    },
    {
        names: 'crossarms',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} crosses their arms.`);
        }
    },
    {
        names: 'cry',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} is crying.`);
        }
    },
    {
        names: 'dance',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} is busting out some moves, some sweet dance moves.`);
        }
    },
    {
        names: ['facepalm', 'upset'],
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} is upset.`);
        }
    },
    {
        names: 'geargrind',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} does the Gear Grind.`);
        }
    },
    {
        names: 'kneel',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} kneels.`);
        }
    },
    {
        names: 'laugh',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} laughs at ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} laughs.`);
        }
    },
    {
        names: 'no',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} disagrees with ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} disagrees.`);
        }
    },
    {
        names: 'point',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} points at ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} points.`);
        }
    },
    {
        names: 'ponder',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} ponders.`);
        }
    },
    {
        names: 'rockout',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} is rocking out!.`);
        }
    },
    {
        names: 'sad',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} is sad.`);
        }
    },
    {
        names: 'salute',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} salutes ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} salutes.`);
        }
    },
    {
        names: 'shiver',
        func: (user, ) => {
            broadcastSystemMessage(`${user.getName()} shivers.`);
        }
    },
    {
        names: 'shrug',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} shrugs at ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} shrugs.`);
        }
    },
    {
        names: 'shuffle',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} does the Inventory Shuffle.`);
        }
    },
    {
        names: 'sit',
        func: (user) => {
            broadcastSystemMessage(`${user.getName()} sits.`);
        }
    },
    {
        names: 'sleep',
        func: (user, args) => {
            broadcastSystemMessage(`${user.getName()} goes to sleep.`);
        }
    },
    {
        names: 'step',
        func: (user, args) => {
            broadcastSystemMessage(`${user.getName()} does the Dodge Step.`);
        }
    },
    {
        names: 'surprised',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} is surprised by ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} is surprised.`);
        }
    },
    {
        names: 'talk',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} is talking to ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} is talking.`);
        }
    },
    {
        names: ['thanks', 'thank', 'thk', 'ty'],
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} thanks ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} is grateful.`);
        }
    },
    {
        names: 'threaten',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} threatens ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} is threatening.`);
        }
    },
    {
        names: 'wave',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} waves at ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} waves.`);
        }
    },
    {
        names: 'yes',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} agrees with ${args}.`);
            else
                broadcastSystemMessage(`${user.getName()} agrees.`);
        }
    },
    {
        names: 'me',
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} ${args}`);
            else
                user.sendSystemMessage('Usage: /me <whatever you like>');
        }
    },
    {
        names: ['bite', 'biteankle'],
        func: (user, args) => {
            if (args)
                broadcastSystemMessage(`${user.getName()} bites ${args}'s ankles.`);
            else
                broadcastSystemMessage(`Watch out! ${user.getName()} is going to start biting the nearest ankle.`);
        }
    },
    {
        names: 'list',
        func: async (user) => {
            let memberList = await listUsers();
            user.sendSystemMessage(`Current online members: ${memberList}`);
        }
    },
    {
        names: 'help',
        func: (user) => {
            user.sendSystemMessage(`The following commands are available: ${commandHandler.allCommands}`);
        }
    },
    {
        names: 'name',
        func: (user) => {
            user.sendSystemMessage('/name is deprecated, so, umm, stop using it.');
        }
    },
    //Admin commands
    {
        names: 'mute',
        permission: 'admin',
        func: (user, target) => {
            //find player

            //set mute status

            //broadcast to admins
            broadcastToAdmins(`${target} was muted by ${user.getName()}`);
        }
    }
]
//#endregion

// Start the TCP server
const server = net.createServer(socket => {

    // Init packet class?
    const packet = new TINYPacket();

    console.log("User connecting");

    //Create user
    const user = new chatUser(socket);

    // Add client to the list
    clients.push(user);

    // Define packet handlers
    packet.on('connect', async (user, packet) => {
        if (packet.key) {
            user.apiKey = packet.key;
            user.clientVersion = packet.version;
            await user.authenticate();
            if(user.isAuthenticated){
                // Send welcome message
                user.sendSystemMessage(`Welcome to the ${config.name} version ${config.version}!`);
                user.sendSystemMessage(`There is currently ${clients.length} users connected`);
            }
        } else {
            user.disconnect({
                type: packets.AUTH,
                valid: false
            });

        }
        //console.log(`New user connected - Key: ${user.apiKey}, Version: ${user.clientVersion}`);
    });
    packet.on('message', (user, packet) => {
        //send to message handler
        if (!packet.message.indexOf('/')) {
            handleCommand(user, packet);
        } else {
            broadcast(user.getName(), packet.message);
        }
    });
    packet.on('enter', enterPacket);
    packet.on('leave', leavePacket);
    packet.on('system', systemPacket);
    packet.on('update', (user, packet) => {
        console.log(`RESV: update from ${packet.name} X:${packet.position.X} Y:${packet.position.Y} Z:${packet.position.Z}`);
        user.updateMumbleData(packet);
    });

    socket.on('data', function (data) {

        packet.handle(data, user);

        //let packet = tPacket.handle(data);
        // if (!packet) {
        //     console.log(`Recieved invalid packet from ${socket.remoteAddress}`);
        //     console.log(data);
        //     socket.write("Bye Bookah!!");
        //     socket.end();

        //     return;
        // }



        // if (user.isAuthenticated == false) {
        //     //non loggedin user
        //     if(packet.type == TINYPacket.CONNECT) {
        //         //handle connection
        //         if (packet.key)
        //             user.apiKey = packet.key;
        //         user.clientVersion = packet.version;
        //         user.isAuthenticated = true;
        //         console.log(`New user connected - Key: ${user.apiKey}, Version: ${user.clientVersion}`);
        //     } else {
        //         socket.write("Bye Bookah!!");
        //         socket.end();
        //     }
        // } else {
        //     //Authenticated User
        //     switch (packet.type) {
        //         case undefined:
        //             console.log("Missing packet type");
        //             socket.end();
        //             return;
        //         case TINYPacket.MESSAGE:
        //             //send to message handler
        //             if (!packet.message.indexOf('/')) {
        //                 handleCommand(user, packet);
        //             } else {
        //                 broadcast(user.getName(), packet.message);
        //             }
        //             break;
        //         case TINYPacket.LEAVE:
        //             //broadcast leave
        //             break;
        //         case TINYPacket.ENTER:
        //             //broadcast enter
        //             console.dir(packet);
        //             break;
        //         case TINYPacket.SYSTEM:
        //             //handle command
        //             break;
        //         case TINYPacket.UPDATE:
        //             //handle location update
        //             user.updateMumbleData(packet);
        //             //socket.info = packet;
        //             //console.dir(packet);
        //             break;
        //         default:
        //             console.log("Unknown packet %j", packet);
        //             socket.end();
        //             return;
        //     }
        // }

    });

    //socket.on('end', function () {
    //    clients.splice(clients.indexOf(socket), 1);
    //    broadcast(socket.name + " left the chat.\n");
    //});

    socket.once('close', function () {
        console.log(`removing user (${user.characterName})`);
        clients.splice(clients.indexOf(user), 1);
        console.log(`Clients = ${clients.length}`);
        //Rando bots
        if (typeof socket.info !== 'undefined')
            broadcast(socket.info.name, `Leaving the chat.`);
    })

    socket.on('error', function (error) {

        switch (error.code) {
            case 'ECONNRESET':
                //ignore, just client disconnecting
                break;

            default:
                console.log(error.message);
                break;
        }

    });

});

const broadcast = (user, message) => {
    console.log(`${user}> ${message}`);
    let packet = {
        "type": packets.MESSAGE,
        "name": user,
        "message": message
    };
    clients.forEach(client => client.sendMessage(packet));
}

const broadcastSystemMessage = (message) => {
    clients.forEach(client => client.sendSystemMessage(message));
}

const broadcastToAdmins = (message) => {
    clients.forEach(client => client.sendSystemMessage(message));
}

//Thanks @once#6585, @Throne3d#2479 
const listUsers = async () => {
    let players = clients.filter(client => client.isAuthenticated).map(client => client.getListName());
    return (await Promise.all(players)).join(', ');
}

const handleCommand = (user, packet) => {

    let matches = packet.message.toString().match(/(\w+)(.*)/);
    let command = matches[1].toLowerCase();
    let args = matches[2].trim();

    try {
        commandHandler.run(user, command, args);
    } catch (error) {
        console.log(tcolors.fg.Red, `${error}: ${command}`, tcolors.Reset);
        user.sendSystemMessage(error);
    }
}

const parseCommad = (line) => {
    let matches = line.match(/(\w+)(.*)/);
    let command = matches[1].toLowerCase();
    let args = matches[2].trim();
    return {
        "command": command,
        "args": args
    };
}

// Load chat commands
console.log(`Loading ${chatCommands.length} chat commands.`);
const commandHandler = new chatCommand(chatCommands);

//Start the server
server.listen(config.port, config.address);

// Put a friendly message on the terminal of the server.
console.log(`Chat server running at port ${config.port} \n`);