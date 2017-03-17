//Basic game structure, setting all available states
//After done game will move to the boot state for pre configurations

    var server;
    var tryConnectInterval;
    var game = new Phaser.Game(GlobalConfiguration.GameWidth, GlobalConfiguration.GameHeight, Phaser.AUTO, 'gameDiv');

    var nextState = function (state) {
        if (game.state.checkState(state)) {
            server.GetInstance().UnregisterForMessages('states', nextState);
            game.staet.start(state);
        }
    }

    var startGame = function () {
        //global variable game for creating the phaser game
        //game will be drawn on the "gameDiv" div in the game.html file
        
        console.log("starting game...");
        //all game states
        game.state.add('boot', bootState);
        game.state.add('load', loadState);
        //game.state.add('random', randomState);
        //game.state.add('moving', movingState);
        //game.state.add('playground', playground);

        GlobalConfiguration.States.forEach(function (state, index, array) {
            game.state.add(state, playground);
        });

        game.state.add('win', winState);
        game.state.add('instructions', instructionsState);

        //go to boot state
        console.log("game.js done, moving to boot state");
        game.state.start('boot');
        
    }

    var connectToServer = function () {
        if (!server.IsConnected()) {
            server.Connect();
            setTimeout(function () {
                if (server.IsConnected()) {
                    console.log("Connected to server...");
                    clearInterval(tryConnectInterval);

                    //server.Send({ Type: Server.Protocol.PLAYER_NAME, Data: 'efi' });

                    server.RegisterForMessages('states', nextState);
                    console.log("try to start game");
                    startGame();
                } else {
                    console.log("Couldn't connect to server, will try again in 5 seconds...");
                }
            }, GlobalConfiguration.TryReconnectToServerInterval)
           
        } else {
            clearInterval(tryConnectInterval);
        }
    }

    //connect to server
    server = Server.GetInstance();
    if (server) {
        console.log("Got server instance, trying to connect...");
        connectToServer();
        //tryConnectInterval = setInterval(connectToServer, 1000);
    } else {
        console.log('error getting server instance on game.js')
    }
