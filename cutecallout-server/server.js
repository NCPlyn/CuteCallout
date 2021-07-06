const WebSocket = require('ws')

texts = [" is cute!","... again being adorable"," is just... awww"," CUTECUTECUTE"," is a good boy!"," == cute",", how are you so cuuteee???"," needs cuddles for being cute!"]

const wss = new WebSocket.Server({ port: 8080 },()=>{
    console.log('server started')
})

wss.on('connection', function connection(ws, req) {
   console.log(req.socket.remoteAddress + " has connected!");
   
   ws.on('message', (data) => {
      console.log('data received \n %o',data)
      ws.send(data+";"+texts[Math.floor(Math.random()*texts.length)]);
   })
})

wss.on('listening',()=>{
   console.log('listening on 8080')
})