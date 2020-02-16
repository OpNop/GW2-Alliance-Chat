const serverInfo = {
    "version": "1.0.0",
    "name": "TINY Alliance Chat Server (TACS)",
    "address": "0.0.0.0",
    "port": "8888"
}
String.prototype.firstWord = function () {
    return this.replace(/\s.*/, '')
}

const net = require('net');
const TINYPacket = require('./src/ParsePacket.js');
const tcolors = require('./src/tinyColor.js').colors;
const tinyPrompt = require('serverline');
const chatCommand = require('./src/chatCommand.js');


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
            console.log(clients.map(q => `${q.info.name} [${q.info.map}]` || ('Unknown user')).join(', '));
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
            let characterSocket = clients.find((client => {
                return client.info.name === character
            }));
            if (!characterSocket) {
                console.log(tcolors.fg.Red, "Character not found", tcolors.Reset);
            } else {
                console.dir(characterSocket.info);
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
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} beckons to ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} beckons.`);
        }
    },
    {
        names: 'bow',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} bows for ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} bows.`);
        }
    },
    {
        names: 'cheer',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} cheers for ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} cheers.`);
        }
    },
    {
        names: 'cower',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} cowers in fear from ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} cowers.`);
        }
    },
    {
        names: 'crossarms',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} crosses their arms.`);
        }
    },
    {
        names: 'cry',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} is crying.`);
        }
    },
    {
        names: 'dance',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} is busting out some moves, some sweet dance moves.`);
        }
    },
    {
        names: ['facepalm', 'upset'],
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} is upset.`);
        }
    },
    {
        names: 'geargrind',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} does the Gear Grind.`);
        }
    },
    {
        names: 'kneel',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} kneels.`);
        }
    },
    {
        names: 'laugh',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} laughs at ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} laughs.`);
        }
    },
    {
        names: 'no',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} disagrees with ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} disagrees.`);
        }
    },
    {
        names: 'point',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} points at ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} points.`);
        }
    },
    {
        names: 'ponder',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} ponders.`);
        }
    },
    {
        names: 'rockout',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} is rocking out!.`);
        }
    },
    {
        names: 'sad',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} is sad.`);
        }
    },
    {
        names: 'salute',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} salutes ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} salutes.`);
        }
    },
    {
        names: 'shiver',
        func: (socket, ) => {
            broadcastSystemMessage(`${socket.info.name} shivers.`);
        }
    },
    {
        names: 'shrug',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} shrugs at ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} shrugs.`);
        }
    },
    {
        names: 'shuffle',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} does the Inventory Shuffle.`);
        }
    },
    {
        names: 'sit',
        func: (socket) => {
            broadcastSystemMessage(`${socket.info.name} sits.`);
        }
    },
    {
        names: 'sleep',
        func: (socket, args) => {
            broadcastSystemMessage(`${socket.info.name} goes to sleep.`);
        }
    },
    {
        names: 'step',
        func: (socket, args) => {
            broadcastSystemMessage(`${socket.info.name} does the Dodge Step.`);
        }
    },
    {
        names: 'surprised',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} is surprised by ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} is surprised.`);
        }
    },
    {
        names: 'talk',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} is talking to ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} is talking.`);
        }
    },
    {
        names: ['thanks', 'thank', 'thk', 'ty'],
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} thanks ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} is grateful.`);
        }
    },
    {
        names: 'threaten',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} threatens ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} is threatening.`);
        }
    },
    {
        names: 'wave',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} waves at ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} waves.`);
        }
    },
    {
        names: 'yes',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} agrees with ${args}.`);
            else
                broadcastSystemMessage(`${socket.info.name} agrees.`);
        }
    },
    {
        names: 'me',
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} ${args}`);
            else
                sendSystemMessage(socket, 'Usage: /me <whatever you like>');
        }
    },
    {
        names: ['bite', 'biteankle'],
        func: (socket, args) => {
            if (args)
                broadcastSystemMessage(`${socket.info.name} bites ${args}'s ankles.`);
            else
                broadcastSystemMessage(`Watch out! ${socket.info.name} is going to start biting the nearest ankle.`);
        }
    },
    {
        names: 'list',
        func: (socket) => {
            sendSystemMessage(socket, clients.map(q => `${q.info.name} [${q.info.map}]` || ('Unknown user')).join(', '));
        }
    },
    {
        names: 'help',
        func: (socket) => {
            sendSystemMessage(socket, `The following commands are available: ${commandHandler.allCommands}`);
        }
    },
    {
        names: 'name',
        func: (socket) => {
            sendSystemMessage(socket, '/name is deprecated, so, umm, stop using it.');
        }
    }
]
//#endregion

// Start the TCP server
const server = net.createServer(socket => {

    // Create internal name
    socket.name = `${socket.remoteAddress}:${socket.remotePort}`;

    // Add client to the list
    clients.push(socket);

    // Send welcome message
    sendSystemMessage(socket, `Welcome to the ${serverInfo.name} version ${serverInfo.version}!`)
    sendSystemMessage(socket, `There is currently ${clients.length} users connected`)

    // Handle incoming messages from clients.
    //packet format
    samplePacket = {
        "type": "message",
        "args": []
    }
    socket.on('data', function (data) {

        let packet = TINYPacket.Parse(data);
        if (!packet) {
            console.log(`Recieved invalid packet from ${socket.remoteAddress}`);
            console.log(data);
            //socket.end();
            return;
        }

        switch (packet.type) {
            case undefined:
                console.log("Missing packet type");
                socket.end();
                return;
            case TINYPacket.MESSAGE:
                //send to message handler
                if (!packet.message.indexOf('/')) {
                    handleCommand(socket, packet);
                } else {
                    broadcast(socket.info.name, packet.message);
                }
                break;
            case TINYPacket.LEAVE:
                //broadcast leave
                break;
            case TINYPacket.ENTER:
                //broadcast enter
                break;
            case TINYPacket.SYSTEM:
                //handle command
                break;
            case TINYPacket.UPDATE:
                //handle location update
                socket.info = packet;
                //console.dir(packet);
                break;
            default:
                console.log("Unknown packet %j", packet);
                socket.end();
                return;
        }
    });

    //socket.on('end', function () {
    //    clients.splice(clients.indexOf(socket), 1);
    //    broadcast(socket.name + " left the chat.\n");
    //});

    socket.once('close', function () {
        clients.splice(clients.indexOf(socket), 1);
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
        "type": TINYPacket.MESSAGE,
        "name": user,
        "message": message
    };
    clients.forEach(client => sendMessage(client, packet));
}

const broadcastSystemMessage = (message) => {
    clients.forEach(client => sendSystemMessage(client, message));
}

const sendMessage = (socket, packet) => {
    socket.write(JSON.stringify(packet));
}

const sendSystemMessage = (to, message) => {
    let packet = {
        "type": TINYPacket.SYSTEM,
        "message": message
    };
    to.write(JSON.stringify(packet));
}

const handleCommand = (socket, packet) => {

    let matches = packet.message.toString().match(/(\w+)(.*)/);
    let command = matches[1].toLowerCase();
    let args = matches[2].trim();

    try {
        commandHandler.run(socket, command, args);
    } catch (error) {
        console.log(tcolors.fg.Red, `${error}: ${command}`, tcolors.Reset);
        sendSystemMessage(socket, error);
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
server.listen(serverInfo.port, serverInfo.address);

// Put a friendly message on the terminal of the server.
console.log(`Chat server running at port ${serverInfo.port} \n`);