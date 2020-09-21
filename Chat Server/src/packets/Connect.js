module.exports = function (user, packet) {

    if (packet.key)
        user.apiKey = packet.key;
    user.clientVersion = packet.version;
    user.isAuthenticated = true;
    console.log(`New user connected - Key: ${user.apiKey}, Version: ${user.clientVersion}`);
    // Send welcome message
    user.sendSystemMessage(`Welcome to the ${serverInfo.name} version ${serverInfo.version}!`);
    user.sendSystemMessage(`There is currently ${clients.length} users connected`);

}