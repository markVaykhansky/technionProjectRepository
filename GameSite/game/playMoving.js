var movingState = (function () {
    var isSet = false;
    var joints = 'undefined';
    var nextStarId = 0;
    var nextStarTimer;
    var currentSkeletonIndex = 1;
    var starsGroup = '';
    var skeletonGroup = '';
    var currentStar;
    var body = [];
    var text;
    var nextStarPoint;
    var nextStarPointDeltas = {X:0,Y:0};
    //Object for holding hits data
    var hits = {};
    

    function endStage() {
        var server = Server.GetInstance();
        server.UnregisterForMessages(Server.Protocol.SKELETON_DATA, updateJoints);
        server.UnregisterForMessages(Server.Protocol.GET_NEXT_STAR_MOVING_POSITION, drawNewStar);
        server.Send({ Type: Server.Protocol.KINECT_STOP });
        game.state.start('instructions');
    }


    function drawNewStar(data) {
        if (data != null) {
            nextStarPoint = data;
        }
        console.log("Creating random star");
        currentStar = starsGroup.create(nextStarPoint.X, nextStarPoint.Y, 'a' + Math.floor(Math.random() * 10 % 3 + 1));
        currentStar.scale.setTo(2.0, 2.0);
        console.log("nextStarId: " + nextStarId);


    };

    function nearestJoint() {
        var closest = 1000;
        var jointIndex;
        for (var i = 0; i < body.length; i++) {
            var dest = Math.sqrt(Math.pow(nextStarPoint.X - body[i].body.position.x, 2), Math.pow(nextStarPoint.Y - body[i].body.position.y, 2));
            if (dest < closest) {
                closest = dest;
                jointIndex = i;
            }
        }
        console.log("Hitting joint index:" + jointIndex);
        return {
            jointIndex: jointIndex,
            skeletonIndex: currentSkeletonIndex
        };
    }


    function getNewStarFromServer() {
        server.Send({ Type: Server.Protocol.GET_NEXT_STAR_MOVING_POSITION });
    }

    function killStar(player, star) {
        var hittingJoint = nearestJoint();
        nextStarId += 1;
        emitter.x = star.x;
        emitter.y = star.y;
        emitter.start(true, 2000, null, 4);
        star.kill();
        var enemiesLeft = GlobalConfiguration.EnemyCount || GlobalConfiguration.movingStarsToWin;
        if (nextStarId < enemiesLeft) {
            text.text = "You got " + nextStarId + " out of " + enemiesLeft;
            server.Send({ Type: Server.Protocol.MOVING_HITS_DATA, Data: JSON.stringify(hittingJoint) });
            game.time.events.add(GlobalConfiguration.movingStarsDelay, getNewStarFromServer);
        } else {
            text.text = "Good! wait for next stage...";
            endStage();
        }
    };

    function updateJoints(data) {
        joints = data.Joints;
        currentSkeletonIndex = data.SkeletonIndex;
        if (data.NextEnemyPoint != null) {
            nextStarPoint = data.NextEnemyPoint;
        }
        isSet = true;
    };

    return {

        create: function () {

            //set background image
            var background = game.add.sprite(0, 0, 'background');

            //add the astroids group
            starsGroup = game.add.group();

            //enable physics on the astroids
            starsGroup.enableBody = true;

            //create an emitter for killing astroids
            emitter = game.add.emitter(0, 0, 300);
            emitter.makeParticles('particle');
            emitter.gravity = 300;
            emitter.setYSpeed(-400, 400);


            skeletonGroup = game.add.group();
            skeletonGroup.enableBody = true;
            for (var i = 0; i < 25; i++) {
                body[i] = skeletonGroup.create(0, 0, 'dot');
            }


            text = game.add.text(10, 10, 'Good Luck!', { font: '30px Courier', fill: '#ffffff' });

            Server.GetInstance().RegisterForMessages(Server.Protocol.SKELETON_DATA, updateJoints);
            Server.GetInstance().RegisterForMessages(Server.Protocol.GET_NEXT_STAR_MOVING_POSITION, drawNewStar);

            game.time.events.add(GlobalConfiguration.movingStarsDelay, getNewStarFromServer);

        },
        update: function () {

            game.physics.arcade.overlap(skeletonGroup, starsGroup, killStar, null, this);

            if (isSet) {
                if (joints != 'undefined') {
                    for (var i = 0; i < joints.length; i++) {

                        body[i].body.position.x = joints[i].X;
                        body[i].body.position.y = joints[i].Y;
                    }
                }
                isSet = false;

            }
            if (currentStar != undefined) {                                     //Update the position of a moving object only if it is in the game area
                if (currentStar.body.position.x < game.world.width - 200 &&
                    currentStar.body.position.y < game.world.height - 100 &&
                    currentStar.body.position.x > 200 &&
                    currentStar.body.position.y > 100){
                    currentStar.body.position.x = nextStarPoint.X;
                    console.log(nextStarPoint.X);
                    currentStar.body.position.y = nextStarPoint.Y;
                    console.log(nextStarPoint.Y);
                }
            }
        }
    }
})();