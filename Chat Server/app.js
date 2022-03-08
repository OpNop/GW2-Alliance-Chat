String.prototype.firstWord = function () {
    return this.replace(/\s.*/, '')
}

const net = require('net');
const express = require('express');
const cors = require('cors');
const apiserver = express();
const TINYPacket = require('./src/packets/Packet.js');
const MessagePacket = require("./src/packets/Message.js");
const EnterPacket = require("./src/packets/Enter.js");
const LeavePacket = require("./src/packets/Leave.js");
const SystemPacket = require('./src/packets/System.js');
const AuthPacket = require('./src/packets/Auth.js');

const config = require('./config.json');
const tcolors = require('./src/tinyColor.js').colors;
const tinyPrompt = require('serverline');
const chatCommand = require('./src/chatCommand.js');
const chatUser = require('./src/user.js');
const chatUserNoApi = require('./src/userNoApi.js');
const apiTest = require('./apiTest');

let apiIsLive = false;

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
let clients = new Map();//made this a map to keep things easier

//#region Chat Commands (dear lord I want to put these someplace else)
const chatCommands = [{
        names: 'beckon',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} beckons to ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} beckons.`));
        }
    },
    {
        names: 'bow',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} bows for ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} bows.`));
        }
    },
    {
        names: 'cheer',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} cheers for ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} cheers.`));
        }
    },
    {
        names: 'cower',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} cowers in fear from ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} cowers.`));
        }
    },
    {
        names: 'crossarms',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} crosses their arms.`));
        }
    },
    {
        names: 'cry',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} is crying.`));
        }
    },
    {
        names: 'dance',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} is busting out some moves, some sweet dance moves.`));
        }
    },
    {
        names: ['facepalm', 'upset'],
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} is upset.`));
        }
    },
    {
        names: 'geargrind',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} does the Gear Grind.`));
        }
    },
    {
        names: 'gg',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} says "Good Game"`));
        }
    },
    {
        names: 'kneel',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} kneels.`));
        }
    },
    {
        names: 'laugh',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} laughs at ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} laughs.`));
        }
    },
    {
        names: 'no',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} disagrees with ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} disagrees.`));
        }
    },
    {
        names: 'playdead',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} is probably dead.`));
        }
    },
    {
        names: 'point',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} points at ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} points.`));
        }
    },
    {
        names: 'ponder',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} ponders.`));
        }
    },
    {
        names: 'rockout',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} is rocking out!.`));
        }
    },
    {
        names: 'sad',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} is sad.`));
        }
    },
    {
        names: 'salute',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} salutes ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} salutes.`));
        }
    },
    {
        names: 'shiver',
        func: (user, ) => {
            broadcast(new SystemPacket(`${user.name} shivers.`));
        }
    },
    {
        names: 'shrug',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} shrugs at ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} shrugs.`));
        }
    },
    {
        names: 'shuffle',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} does the Inventory Shuffle.`));
        }
    },
    {
        names: 'sit',
        func: (user) => {
            broadcast(new SystemPacket(`${user.name} sits.`));
        }
    },
    {
        names: ['sleep', 'nap'],
        func: (user, args) => {
            broadcast(new SystemPacket(`${user.name} goes to sleep.`));
        }
    },
    {
        names: 'step',
        func: (user, args) => {
            broadcast(new SystemPacket(`${user.name} does the Dodge Step.`));
        }
    },
    {
        names: 'stretch',
        func: (user, args) => {
            broadcast(new SystemPacket(`${user.name} is streching.`));
        }
    },
    {
        names: 'surprised',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} is surprised by ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} is surprised.`));
        }
    },
    {
        names: 'talk',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} is talking to ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} is talking.`));
        }
    },
    {
        names: ['thanks', 'thank', 'thk', 'ty'],
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} thanks ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} is grateful.`));
        }
    },
    {
        names: 'threaten',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} threatens ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} is threatening.`));
        }
    },
    {
        names: 'wave',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} waves at ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} waves.`));
        }
    },
    {
        names: 'yes',
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} agrees with ${args}.`));
            else
                broadcast(new SystemPacket(`${user.name} agrees.`));
        }
    },
    {
        names: ['me', 'e'],
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} ${args}`));
            else
                user.sendSystemMessage('Usage: /me <whatever you like>');
        }
    },
    {
        names: ['bite', 'biteankle'],
        func: (user, args) => {
            if (args)
                broadcast(new SystemPacket(`${user.name} bites ${args}'s ankles.`));
            else
                broadcast(new SystemPacket(`Watch out! ${user.name} is going to start biting the nearest ankle.`));
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
            broadcastToAdmins(`${target} was muted by ${user.name}`);
        }
    }
]
//#endregion

// Start the TCP server
const server = net.createServer(async function(socket) {

    apiIsLive = await apiTest();
    let user;

    // Init packet class?
    const packet = new TINYPacket();

    console.log("User connecting");

    //Create user
    if(apiIsLive){
        user = new chatUser(socket);
        console.log(`Created new user id ${user.id} with a live API`)
    }
    else {
        user = new chatUserNoApi(socket)
        console.log(`Created new user id ${user.id} with a dead API`)
    }

    // Add client to the list
    clients.set(user.id, user);

    // Define packet handlers
    packet.on('connect', onConnect);
    packet.on('message', onMessage);
    packet.on('update', (user, packet) => {
        user.updateMumbleData(packet);
    });

    socket.on('data', function (data) {
        packet.handle(data, user);
    });

    socket.once('close', function () {
        console.log(`removing user (${user.characterName})`);
        clients.delete(user.id);
        console.log(`Clients = ${clients.size}`);
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
    if(packet.key){
        console.log(`User ${user.id} is trying to auth with ${packet.key}`);
    }
    //Version check
    if (packet.version != config.version && packet.version != '1.0.3.0') {
        user.disconnect(new AuthPacket(false, "Old version, please update. https://tinyarmy.org/tacs/"));
        return;
    }

    //API check
    if (!packet.key) {
        user.disconnect(new AuthPacket(false, "Missing API key"));
        return;
    }

    // Guild check
    //if(apiIsLive){
        //If the GW2 API is inaccessible, we cannot verify [TINY] membership; just assume users are in [TINY].
        user.apiKey = packet.key;
        user.clientVersion = packet.version;
        await user.authenticate();
        if (!user.isAuthenticated) {
            user.disconnect(new AuthPacket(false, "Bookah!!! you are not a TINY, what are you doing here!"));
            return;
        }
    //}

    // Everything passed Send welcome message
    user.sendPacket(new SystemPacket(`Welcome to the ${config.name} version ${config.version}!`));
    if (clients.size === 1) {
        user.sendPacket(new SystemPacket(`There's no one else here! The lab is all yours... for now. Please keep the explosions to a minimum.`));
    } else {
        user.sendPacket(new SystemPacket(`There is currently ${userCount()} users connected`));
    }

    //Broadcast Joined message
    if(apiIsLive){
        broadcast(new EnterPacket(user.accountName), false);
    }
    else{
        broadcast(new EnterPacket("A new user"), false);
    }
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
        broadcast(new MessagePacket(user.name, packet.message));
    }
}
//#endregion

const authedUsers = (ignoreAuth = false)=> {
    if(!apiIsLive || ignoreAuth){
        return [...clients.values()];
    }
    return [...clients.values()].filter(client => client.isAuthenticated);
}

const userCount = () => {
    return authedUsers().length;
}    

const broadcast = (packet, toSelf = true) => {
    authedUsers().forEach(client => client.sendPacket(packet));
}

const broadcastToAdmins = (message) => {
    clients.forEach(client => client.sendSystemMessage(message));
}

//Thanks @once#6585, @Throne3d#2479 
const listUsers = async () => {
    let players = authedUsers().map(client => client.getListName());
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
    let geoJSON = authedUsers(true).filter(client => client.mumbleData).map(client => {
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