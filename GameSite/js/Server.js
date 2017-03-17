var Server = (function () {

    //private
    var instance;
    function server() {
        //private handlers
        var socket;
        var eventsDictionary = {};

        function onopen(event) {
            console.log('Opened connection to server');
        }

        function onclose(event) {
            console.log('closed connetion to server');
            eventsDictionary = {};
        }

        function onmessage(event) {
            var message = JSON.parse(event.data);
            if (message.Type in eventsDictionary) {
                eventsDictionary[message.Type].forEach(function (cb) {
                    cb(message.Data);
                })
            }
        }

        function onerror(err) {
            console.log(err);
        }

        //public api
        this.IsConnected = function () {
            return socket? socket.readyState == 1:false;
        }

        this.Connect = function () {
            try{
                socket = new WebSocket("ws://localhost:2012");
                socket.onopen = onopen;
                socket.onclose = onclose;
                socket.onmessage = onmessage;
                socket.onerror = onerror;
            }catch(err){
                console.log("couldn't open socket, error: " + err);
            }
        };

        this.Close = function () {
            socket.close();
            socket = 'undefined';
        };

        this.Send = function (message) {
            socket.send(JSON.stringify(message));
        }

        this.RegisterForMessages = function (messageType, cb) {
            if (!(messageType in eventsDictionary)) {
                eventsDictionary[messageType] = [];
            }
            eventsDictionary[messageType].push(cb);
        }

        this.UnregisterForMessages = function (messageType, cb) {
            if (!(messageType in eventsDictionary)) {
                return;
            }
            var index = eventsDictionary[messageType].indexOf(cb);
            if (index != -1) {
                eventsDictionary[messageType].splice(index, 1);
            }
        }

    }

    var Protocol = {
        STATES:1,
        KINECT_START: 10,
        KINECT_STOP: 11,
        KINECT_CHANGED_AVAILABILITY: 12,

        GET_NEXT_INSTRUCTIONS : 100,
        GET_NEXT_STAR_POSITION: 101,
        GET_SAME_STAR_POSITION:102,

        SKELETON_DATA : 200,
        HITS_DATA: 201,

        GAME_DONE: 301,
        PLAYER_NAME: 400,

        DISP_PROB: 500
    }

    //public
    return {
        GetInstance: function () {
            if (!instance) {
                instance = new server();
            }
            return instance;
        },
        Protocol:Protocol
    }

})();


