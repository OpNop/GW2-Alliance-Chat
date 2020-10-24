const packet = require('./Packet.js').packets;

module.exports = class Message {
    
    constructor(name, message){
        this.type = packet.MESSAGE;
        this.name = name
        this.message = message;
    }
}