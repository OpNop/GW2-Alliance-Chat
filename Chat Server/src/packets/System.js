const packet = require('./Packet.js').packets;

module.exports = class System {
    
    constructor(message){
        this.type = packet.SYSTEM;
        this.message = message;
    }
}