const serverInfo = {
    "version": "1.0.0",
    "name": "TINY Alliance Chat Server (TACS)",
    "address": "0.0.0.0",
    "port": "8888"
}

const net = require('net');
const readline = require('readline');
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
    socket.write(`Welcome to the ${serverInfo.name} version ${serverInfo.version}!`);
    socket.write(`There is currently ${clients.length} users connected`);

    // Handle incoming messages from clients.
    socket.on('data', function (data) {
        if (data.indexOf('/') == 0) {
            // Command
            let matches = data.toString().match(/(\w+)(.*)/);
            let command = matches[1].toLowerCase();
            let args = matches[2].trim();

            switch (command) {
                case "name":
                    socket.name = args;
                    sendSystemMessage(socket, `Changed name to ${args}`);
                    break;
                case "list":
                    sendSystemMessage(socket, Array.prototype.map.call(clients, s => s.name).toString());
                    break;
                default:
                    sendSystemMessage(socket, `Unknown command '${command}'`);
                    sendSystemMessage(socket, "Current commands are: /name <name>, /list");
            }

        } else {
            broadcast(data, socket);
        }
    });

    //socket.on('end', function () {
    //    clients.splice(clients.indexOf(socket), 1);
    //    broadcast(socket.name + " left the chat.\n");
    //});

    socket.once('close', function () {
        let name = { "name": socket.name };
        clients.splice(clients.indexOf(socket), 1);
        broadcast(`${socket.name} left the chat.`, socket);
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

const broadcast = (message, user) => {
    console.log(`${user.name}> ${message}`);
    clients.forEach(client => sendMessage(client, message, user));
}

const sendMessage = (socket, message, user) => {
    socket.write(`${user.name}> ${message}`);
}

const sendSystemMessage = (to, message) => {
    to.write(`SERVER> ${message}`);
}

server.listen(serverInfo.port, serverInfo.address);

// Put a friendly message on the terminal of the server.
console.log(`Chat server running at port ${serverInfo.port} \n`);
