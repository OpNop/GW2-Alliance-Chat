const packet = require('./Packet.js').packets;

module.exports = class Enter {
    
    constructor(user){
        this.type = packet.LEAVE;
        this.user = user;
    }
}