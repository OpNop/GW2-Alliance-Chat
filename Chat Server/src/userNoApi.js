const crypto = require("crypto");
const packet = require('./packets/Packet.js').packets;
module.exports = class User {

    constructor(socket) {
        this.socket = socket;
        this.id = crypto.randomBytes(8).toString('base64');

        //set default attributes
        this.characterName = this.id;
        this.mumbleData = null;//Can we make this private?
        this._isOnline = false;
        this.accountName = "";
        this.hasAPIData = false;
        this.permission = "user";
        this.isMuted = false;
        this.clientVersion = "Unknown";
    }

    /**
     * TODO: fill out how this is different from `getListName()`.
     * @returns {String} The... name?
     */
    get name() {
        return this.mumbleData.name;
    }

    /**
     * TODO: fill out how this is different from `getName()`.
     * This needs to be async to fit with the regular (API-live) user's async "getter".
     * @returns {String} The... list name?
     */
    getListName() {
        return `${this.characterName} (unverified)`;
    }

    /**
     * Set the user's online status
     * @param {Object} [mumbleData] Optional mumble data, if setting online. Otherwise sets to offline
     */
    set online(mumbleData = {}) {
        if (mumbleData) {
            this.mumbleData = val;
        }
        this.isOnline = !!val;
    }

    updateMumbleData(mumbleData) {
        this._isOnline = true;
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