const https = require('https')
const options = {
  hostname: 'api.guildwars2.com',
  port: 443,
  path: '/v2/achievements/daily',
  method: 'GET'
}

module.exports = function(){
    return new Promise(resolve=>{
        let status = 200;
        let req = https.request(options, response => {
            response.on('data', ()=> {
                status = response.statusCode;
            })
            response.on('end', () => {
                resolve(status < 400);
            });

        });
        req.on('error', () => {
            resolve(false);
        })

        req.end();
    });
}