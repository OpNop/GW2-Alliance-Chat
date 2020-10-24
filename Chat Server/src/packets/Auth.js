const packet = require('./Packet.js').packets;

module.exports = class Auth {
    
    constructor(valid, reason = null){
        this.type = packet.AUTH;
        this.valid = valid;
        if(reason)
            this.reason = reason;
    }
}