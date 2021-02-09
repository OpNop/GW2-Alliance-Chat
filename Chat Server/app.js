String.prototype.firstWord = function () {
    return this.replace(/\s.*/, '')
}

const net = require('net');
const express = require('express');
const cors = require('cors');
const apiserver = express();
const TINYPacket = require('./src/packets/Packet.js');
const packets = require("./src/packets/Packet").packets;
const connectPacket = require("./src/packets/Connect.js");
const MessagePacket = require("./src/packets/Message.js");
const EnterPacket = require("./src/packets/Enter.js");
const LeavePacket = require("./src/packets/Leave.js");
const SystemPacket = require('./src/packets/System.js');
const updatePacket = require("./src/packets/Update.js");
const AuthPacket = require('./src/packets/Auth.js');

const config = require('./config.json');
const tcolors = require('./src/tinyColor.js').colors;
const tinyPrompt = require('serverline');
const chatCommand = require('./src/chatCommand.js');
const chatUser = require('./src/user.js');
//const { default: SystemMessage } = require('./src/packets/System.js');



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
                broadcast(new SystemPacket(`${user.getName()} beckons to ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} beckons.`));
        }
    },
    {
        names: 'bow',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} bows for ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} bows.`));
        }
    },
    {
        names: 'cheer',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} cheers for ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} cheers.`));
        }
    },
    {
        names: 'cower',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} cowers in fear from ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} cowers.`));
        }
    },
    {
        names: 'crossarms',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} crosses their arms.`));
        }
    },
    {
        names: 'cry',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} is crying.`));
        }
    },
    {
        names: 'dance',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} is busting out some moves, some sweet dance moves.`));
        }
    },
    {
        names: ['facepalm', 'upset'],
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} is upset.`));
        }
    },
    {
        names: 'geargrind',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} does the Gear Grind.`));
        }
    },
    {
        names: 'gg',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} says "Good Game"`));
        }
    },
    {
        names: 'kneel',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} kneels.`));
        }
    },
    {
        names: 'laugh',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} laughs at ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} laughs.`));
        }
    },
    {
        names: 'no',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} disagrees with ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} disagrees.`));
        }
    },
    {
        names: 'playdead',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} is probably dead.`));
        }
    },
    {
        names: 'point',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} points at ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} points.`));
        }
    },
    {
        names: 'ponder',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} ponders.`));
        }
    },
    {
        names: 'rockout',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} is rocking out!.`));
        }
    },
    {
        names: 'sad',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} is sad.`));
        }
    },
    {
        names: 'salute',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} salutes ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} salutes.`));
        }
    },
    {
        names: 'shiver',
        func: (user, ) => {
            broadcast(new SystemPacket(`${user.getName()} shivers.`));
        }
    },
    {
        names: 'shrug',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} shrugs at ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} shrugs.`));
        }
    },
    {
        names: 'shuffle',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} does the Inventory Shuffle.`));
        }
    },
    {
        names: 'sit',
        func: (user) => {
            broadcast(new SystemPacket(`${user.getName()} sits.`));
        }
    },
    {
        names: ['sleep', 'nap'],
        func: (user, args) => {
            broadcast(new SystemPacket(`${user.getName()} goes to sleep.`));
        }
    },
    {
        names: 'step',
        func: (user, args) => {
            broadcast(new SystemPacket(`${user.getName()} does the Dodge Step.`));
        }
    },
    {
        names: 'stretch',
        func: (user, args) => {
            broadcast(new SystemPacket(`${user.getName()} is streching.`));
        }
    },
    {
        names: 'surprised',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} is surprised by ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} is surprised.`));
        }
    },
    {
        names: 'talk',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} is talking to ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} is talking.`));
        }
    },
    {
        names: ['thanks', 'thank', 'thk', 'ty'],
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} thanks ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} is grateful.`));
        }
    },
    {
        names: 'threaten',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} threatens ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} is threatening.`));
        }
    },
    {
        names: 'wave',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} waves at ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} waves.`));
        }
    },
    {
        names: 'yes',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} agrees with ${args}.`));
            else
                broadcast(new SystemPacket(`${user.getName()} agrees.`));
        }
    },
    {
        names: 'me',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} ${args}`));
            else
                user.sendSystemMessage('Usage: /me <whatever you like>');
        }
    },
    {
        names: ['bite', 'biteankle'],
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.getName()} bites ${args}'s ankles.`));
            else
                broadcast(new SystemPacket(`Watch out! ${user.getName()} is going to start biting the nearest ankle.`));
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
    packet.on('connect', onConnect);
    packet.on('message', onMessage);
    packet.on('update', (user, packet) => {
        user.updateMumbleData(packet);
    });

    socket.on('data', function (data) {
        packet.handle(data, user);
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
        if (user.isAuthenticated)
            broadcast(new LeavePacket(user.accountName));
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

//#region Packet Handlers
const onConnect = async (user, packet) => {
    //Version check
    if (packet.version != config.version) {
        user.disconnect(new AuthPacket(false, "Old version, please update. https://tinyarmy.org/tacs/"));
        return;
    }

    //API check
    if (!packet.key) {
        user.disconnect(new AuthPacket(false, "Missing API key"));
        return;
    }

    // Guild check
    user.apiKey = packet.key;
    user.clientVersion = packet.version;
    await user.authenticate();
    if (!user.isAuthenticated) {
        user.disconnect(new AuthPacket(false, "Bookah!!! you are not a TINY, what are you doing here!"));
        return;
    }

    // Everything passed Send welcome message
    user.sendPacket(new SystemPacket(`Welcome to the ${config.name} version ${config.version}!`));
    if (clients.length == 1) {
        user.sendPacket(new SystemPacket(`There's no one else here! The lab is all yours... for now. Please keep the explosion's to a minimum.`));
    } else {
        user.sendPacket(new SystemPacket(`There is currently ${clients.length} users connected`));
    }

    //Broadcast Joined message
    broadcast(new EnterPacket(user.accountName), false);
}

const onMessage = (user, packet) => {
    //send to message handler
    if (!packet.message.indexOf('//')) {
        //Admin command
        //handleAdminCOmmand(user, packet);
    } else if (!packet.message.indexOf('/')) {
        //User command
        handleCommand(user, packet);
    } else {
        //Normal message
        broadcast(new MessagePacket(user.getName(), packet.message));
    }
}
//#endregion

const broadcast = (packet, toSelf = true) => {
    clients.filter(client => client.isAuthenticated).forEach(client => client.sendPacket(packet));
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

apiserver.use(cors());

apiserver.use(express.static('public'));

apiserver.get('/update', (req, res) => {
    let geoJSON = clients.filter(client => client.mumbleData).map(client => {
        let result = {
            type: "Feature",
            geometry: {
                type: "Point",
                coordinates: [
                    parseFloat(client.mumbleData.position.X),
                    parseFloat(client.mumbleData.position.Y)
                ]
            },
            properties: {
                name: client.characterName,
                class: client.mumbleData.eliteSpec || client.mumbleData.profession,
                ip: client.mumbleData.serverAddress,
                id: client.id
            }
        };
        return result;
    })
    res.json(geoJSON);
})

// Load chat commands
console.log(`Loading ${chatCommands.length} chat commands.`);
const commandHandler = new chatCommand(chatCommands);

//Start the server
server.listen(config.port, config.address);

//Start the API server
apiserver.listen(config.apiport, () => {
    console.log(`API server running on port ${config.apiport} \n`);
})

// Put a friendly message on the terminal of the server.
console.log(`Chat server running at port ${config.port} \n`);