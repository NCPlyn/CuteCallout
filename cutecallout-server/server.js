const WebSocket = require('ws');
var wait = false;

texts = [" is cute!","... again being adorable"," is just... awww"," CUTECUTECUTE"," is a good boy!"," == cute",", how are you so cuuteee???"," needs cuddles for being cute!"];

const wss = new WebSocket.Server({ port: 8950 },()=>{
    console.log('Server started');
});

wss.on('connection', function connection(ws, req) {
	var userIP = req.socket.remoteAddress;
	console.log("\x1b[35mConnect: \x1b[0m"+userIP);
   
	ws.on('message', (data) => {
		if(!wait) {
			console.log("\x1b[34mRecieve: \x1b[0m'"+data+"' \x1b[36mfrom \x1b[0m"+userIP);
			var outMsg = data+";"+texts[Math.floor(Math.random()*texts.length)];
			console.log("\x1b[32mSending: \x1b[0m'"+outMsg+"'");
			wss.clients.forEach(function each(client) {
				client.send(outMsg);
			});
			wait=true;
			setTimeout(function(){
				wait=false;
			}, 6000);
		} else {
			console.log("\x1b[31mBlocked: \x1b[0m'"+data+"' \x1b[36mfrom \x1b[0m"+userIP);
		}
    });
});

wss.on('listening',()=>{
   console.log('Listening...')
});