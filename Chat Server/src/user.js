const crypto = require("crypto");
const packet = require('./packets/Packet.js').packets;
const gw2api = require('gw2api-client');
const cacheMemory = require('gw2api-client/src/cache/memory')
const config = require('../config.json')
const packets = require('./packets/Packet').packets
const AuthPacket = require('./packets/Auth.js');

module.exports = class User {

    constructor(socket) {
        //GW2 API client
        this.api = gw2api();
        this.api.cacheStorage(cacheMemory());

        this.socket = socket;
        this.id = crypto.randomBytes(8).toString('base64');

        //set default attributes
        this.characterName = this.id;
        this.isAuthenticated = false;
        this.mumbleData = null;
        this.isOnline = false;
        this.accountName = "";
        this.hasAPIData = false;
        this.permission = "user";
        this.isMuted = false;
        this.apiKey = "";
        this.clientVersion = "Unknown";
    }

    get name() {
        let name = this.accountName;
        if (this.isAuthenticated) {
            if (this.isOnline)
                name = this.characterName;
            else if (this.hasAPIData)
                name = this.accountName;
        }
        return name;
    }

    async getLocation() {
        if (this.isOnline) {
            let map = await this.api.maps().get(this.mumbleData.map);
            return `[${map.name}]`;
        } else {
            return "[Offline]";
        }
    }

    async getListName() {
        if (this.isAuthenticated) {
            let name = this.name;
            let location = await this.getLocation()
            return `${name} ${location}`;
        }
    }

    async authenticate() {
        this.api.authenticate(this.apiKey);
        let account = await this.api.account().get();
        let guilds = account.guilds;
        this.accountName = account.name;

        //check for valid guild
        for (let i = 0; i < config.guilds.length; i++) {
            if (guilds.includes(config.guilds[i])) {
                this.isAuthenticated = true;
                this.sendMessage(new AuthPacket(true));
                return;
            }
        }
        //not found, kick them
        this.disconnect(new AuthPacket(false, "Not in TINY"));
    }

    setOffline() {
        this.mumbleData = null;
        this.isOnline = false;
    }

    setOnline() {
        this.isOnline = true;
    }

    updateMumbleData(mumbleData) {
        this.isAuthenticated = true;
        this.isOnline = true;
        this.mumbleData = mumbleData;
        this.characterName = this.mumbleData.name;
    }

    sendSystemMessage(message) {
        let systemMessage = {
            "type": packet.SYSTEM,
            "message": message
        };
        this.socket.write(JSON.stringify(systemMessage) + '\r');
    }

    sendMessage(packet) {
        this.socket.write(JSON.stringify(packet) + '\r');
    }

    sendPacket(packet) {
        this.socket.write(JSON.stringify(packet) + '\r');
    }

    disconnect(packet) {
        this.socket.end(JSON.stringify(packet));
    }
}