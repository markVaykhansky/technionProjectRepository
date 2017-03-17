//Boot state currently only loading the physics system
//create function is called automatically by phaser calling convention
var instructionsState = (function () {
    var instructionTimer = GlobalConfiguration.InstructionTimer;
    var kinectState = 'Not Ready';
    var instructionText = '';
    var nextState = 'undefined';
    var nextStageTimer;

    function setTextToScreen() {
        var str = '';
        if (nextState != 'win') {
            str = 'Game will start in: ' + instructionTimer + ' Seconds\n';
            str += instructionText;
            str += '\nKinect State: ' + kinectState;
        } else {
            str = "We're saving your movement data\nPlease wait";
        }
        instructionsLabel.text = str;
    }

    function handleKinectStateChanged(state) {
        if (state) {
            kinectState = 'Ready';
        } else {
            kinectState = 'Not Ready';
        }
    }

    function loadNextStage() {
        if (nextState != 'undefined') {
            instructionTimer -= 1;
            console.log("instructionTimer=" + instructionTimer);
            if (instructionTimer < 1) {
                game.time.events.remove(nextStageTimer);
                console.log("Stopped nextStageTimer");
                server.UnregisterForMessages(Server.Protocol.GET_NEXT_INSTRUCTIONS, updateInstructions);
                server.UnregisterForMessages(Server.Protocol.KINECT_CHANGED_AVAILABILITY, handleKinectStateChanged);
                console.log("Try start next state");
                game.state.start(nextState)

            } else {
                setTextToScreen();
            }
        }
    }

    function updateInstructions(instruction) {
        console.log("New instructions set: " + instruction);
        instructionText = instruction.Text;
        nextState = instruction.State;
        if (instruction.EnemyCount) {
            GlobalConfiguration.EnemyCount = instruction.EnemyCount;
        }
    }

    return {
        create: function () {
            instructionTimer = GlobalConfiguration.InstructionTimer;
            instructionsLabel = game.add.text(80, 150, 'Waiting for server...', { font: '30px Courier', fill: '#ffffff' });
            server = Server.GetInstance();
            if (server) {
                server.RegisterForMessages(Server.Protocol.GET_NEXT_INSTRUCTIONS, updateInstructions);
                server.RegisterForMessages(Server.Protocol.KINECT_CHANGED_AVAILABILITY, handleKinectStateChanged);
                server.Send({ Type: Server.Protocol.KINECT_START });
                server.Send({ Type: Server.Protocol.GET_NEXT_INSTRUCTIONS });
                nextStageTimer = game.time.events.loop(Phaser.Timer.SECOND, loadNextStage);
                console.log("Started nextStageTimer");

            } else {
                console.log('Error getting server in instructions state');
            }

        }
    };
})();

