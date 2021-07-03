const WebSocket = require('ws')
const wss = new WebSocket.Server({ port: 8080 },()=>{
    console.log('server started')
})
wss.on('connection', function connection(ws, req) {
   console.log(req.socket.remoteAddress + " has connected!");
   ws.on('message', (data) => {
      console.log('data received \n %o',data)
      ws.send(data+" is cute!");
   })
})
wss.on('listening',()=>{
   console.log('listening on 8080')
})