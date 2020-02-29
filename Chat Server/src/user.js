const crypto = require("crypto");
const packet = require('./ParsePacket.js');
const gw2api = require('gw2api-client');
const cacheMemory = require('gw2api-client/src/cache/memory')

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
    }

    async getName() {
        if(this.isAuthenticated){
            if(this.isOnline)
                return `${this.characterName}`;
            else if(this.hasAPIData)
                return `${this.accountName}`;
        }
        else
            return this.id;
    }

    async getLocation() {
        if(this.isOnline){
            let map = await this.api.maps().get(this.mumbleData.map);
            return `[${map.name}]`;
        } else {
            return "[Offline]";
        }
    }

    async getListName() {
        if(this.isAuthenticated){
            let name = await this.getName();
            let location = await this.getLocation()
            return `${name} ${location}`;
        }
    }

    setOffline() {
        this.mumbleData = null;
        this.isOnline = false;
    }

    setOnline() {
        this.isOnline = true;
    }

    updateMumbleData(mumbleData){
        this.isAuthenticated = true;
        this.isOnline = true;
        this.mumbleData = mumbleData;
        this.characterName = this.mumbleData.name;
    }

    sendSystemMessage(message){
        let systemMessage = {
            "type": packet.SYSTEM,
            "message": message
        };
        this.socket.write(JSON.stringify(systemMessage) + '\r');
    }
}