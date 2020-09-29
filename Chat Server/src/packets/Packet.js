const { EventEmitter } = require('events')

module.exports = class Packet extends EventEmitter {

    constructor() {
        super();
        //this.user = user;
    }

    handle(packet, user) {
        try {
            let parsedPacket = JSON.parse(packet);
            //console.log(`Handeling packet ${parsedPacket.type}`);
            switch (parsedPacket.type){
                case module.exports.packets.MESSAGE:
                    this.emit('message', user, parsedPacket);
                    break;
                case module.exports.packets.ENTER:
                    this.emit('enter', user, parsedPacket);
                    break;
                case module.exports.packets.LEAVE:
                    this.emit('leave', user, parsedPacket);
                    break;
                case module.exports.packets.SYSTEM:
                    this.emit('system', user, parsedPacket);
                    break;
                case module.exports.packets.UPDATE:
                    this.emit('update', user, parsedPacket);
                    break;
                case module.exports.packets.CONNECT:
                    this.emit('connect', user, parsedPacket);
                    break;
            }
        } catch (e) {
            return false;
        }
    }
    
}

module.exports.packets = {
    MESSAGE: 0,
    ENTER: 1,
    LEAVE: 2,
    SYSTEM: 3,
    UPDATE: 4,
    CONNECT: 5,
    AUTH:6
};

// exports.type = [
//     'MESSAGE',
//     'ENTER',
//     'LEAVE',
//     'SYSTEM',
//     'UPDATE',
//     'CONNECT'
// ];



exports.MESSAGE = 0;
exports.ENTER = 1;
exports.LEAVE = 2;
exports.SYSTEM = 3;
exports.UPDATE = 4;
exports.CONNECT = 5;

// exports.Parse = (packet) => {
//     try {
//         return JSON.parse(packet);
//     } catch (e) {
//         return false;
//     }
// }

// exports.BuildPacket = (type, data) => {
//     return; 
// }