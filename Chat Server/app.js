const serverInfo = {
    "version": "1.0.0",
    "name": "TINY Alliance Chat Server (TACS)",
    "address": "0.0.0.0",
    "port": "8888"
}

const net = require('net');
const readline = require('readline');
const TINYPacket = require('./src/ParsePacket.js');
const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

// Keep track of the chat clients
let clients = [];

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
                if(!packet.message.indexOf('/')){
                    handleCommand(socket, packet);
                } else {
                    broadcast(packet.name, packet.message);
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
        broadcast(socket.name, `Leaving the chat.`);
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

    switch (command) {
        case "name":
            socket.name = args;
            sendSystemMessage(socket, `Changed name to ${args}`);
            break;
        case "list":
            sendSystemMessage(socket, clients.map(q => q.name || ('Unknown user')).join(', '));
            break;
        default:
            sendSystemMessage(socket, `Unknown command '${command}'`);
            sendSystemMessage(socket, "Current commands are: /name <name>, /list");
    }
}

server.listen(serverInfo.port, serverInfo.address);

// Put a friendly message on the terminal of the server.
console.log(`Chat server running at port ${serverInfo.port} \n`);