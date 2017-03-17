var playground = (function () {
    var isSet = false;
    var joints = 'undefined';
    var nextStarId = 0;
    var nextStarTimer;
    var currentSkeletonIndex = 1;
    var starsGroup = '';
    var skeletonGroup = '';
    var currentStar;
    var body = [];
    var oldBody = [];
    var text;
    var nextStarPoint;
    var nextStarPointDeltas = { X: 0, Y: 0 };
    var hits = {};
    var slicesGroup;
    var slices = [];
    var newSlices = null;
    var newRects = null;
    var radius = 0.25;
    var probText;
    var porbability = 0.0;

    function endStage() {
        var server = Server.GetInstance();
        server.UnregisterForMessages(Server.Protocol.SKELETON_DATA, updateJoints);
        server.UnregisterForMessages(Server.Protocol.GET_NEXT_STAR_POSITION, drawNewStar);
        server.Send({ Type: Server.Protocol.KINECT_STOP });
        game.state.start('instructions');
    }

    function starOutOfScreen(star) {
        var hittingJoint = {
            jointIndex: -1,
            skeletonIndex: -1
        }
        star.kill();
        server.Send({ Type: Server.Protocol.HITS_DATA, Data: JSON.stringify(hittingJoint) })
        game.time.events.add(GlobalConfiguration.movingStarsDelay, getSameStarFromServer);
    }


    function drawNewStar(data) {
        if (data != null) {
            nextStarPoint = data;
        }
        console.log("Creating star");
        currentStar = starsGroup.create(nextStarPoint.X, nextStarPoint.Y, 'a' + Math.floor(Math.random() * 10 % 3 + 1));
        //currentStar.anchor.setTo(0.5, 0.5);
        //currentStar.scale.setTo(radius / (currentStar.body.width / 2), radius / (currentStar.body.height / 2));
        currentStar.scale.setTo(GlobalConfiguration.RadiusFactor, GlobalConfiguration.RadiusFactor);
        currentStar.checkWorldBounds = true;
        currentStar.events.onOutOfBounds.add(starOutOfScreen, this)
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
        server.Send({ Type: Server.Protocol.GET_NEXT_STAR_POSITION });
    }

    function getSameStarFromServer() {
        server.Send({ Type: Server.Protocol.GET_SAME_STAR_POSITION });
    }

    function killStar(player, star) {
        var hittingJoint = nearestJoint();
        nextStarId += 1;
        emitter.x = star.x;
        emitter.y = star.y;
        emitter.start(true, 2000, null, 4);
        star.kill();
        var enemiesLeft = GlobalConfiguration.EnemyCount || GlobalConfiguration.movingStarsToWin;
        server.Send({ Type: Server.Protocol.HITS_DATA, Data: JSON.stringify(hittingJoint) });
        if ((nextStarId < enemiesLeft) 
            || ((porbability < GlobalConfiguration.sufficientProbability) 
            && (GlobalConfiguration.recordMode === false))
            && (nextStarId < GlobalConfiguration.maxIterations)) {
            text.text = "You got " + nextStarId + " out of " + enemiesLeft;
                game.time.events.add(GlobalConfiguration.movingStarsDelay, getNewStarFromServer);
        } else {
            text.text = "Good! wait for next stage...";
            endStage();
        }
    };

    function printProb(probabilityData) {
        porbability = parseFloat(probabilityData.split(':')[1]);
        probText.text = "Current guess: " + probabilityData.split(':')[0] + "Probability" + probabilityData.split(':')[1];
    };

    function updateJoints(data) {
        joints = data.Joints;
        currentSkeletonIndex = data.SkeletonIndex;
        if (data.NextEnemyPoint != null) {
            nextStarPoint = data.NextEnemyPoint;
            nextStarPoint.X -= GlobalConfiguration.RadiusFactor * 19;
            nextStarPoint.Y -= GlobalConfiguration.RadiusFactor * 19;
        }
        if (data.Slices != null) {
            newSlices = data.Slices;
        }
        if (data.Rects != null) {
            newRects = data.Rects;
        }
        if (data.RadiusPoint != null) {
            var rp = data.RadiusPoint;
            var dist = Math.sqrt(Math.pow(rp.X - data.NextEnemyPoint.X, 2) + Math.pow(rp.Y - data.NextEnemyPoint.Y, 2));
            radius = dist;
        }
        isSet = true;
    };

    function initStage() {
        isSet = false;
        joints = 'undefined';
        nextStarId = 0;
        nextStarTimer;
        currentSkeletonIndex = 1;
        starsGroup = '';
        skeletonGroup = '';
        currentStar;
        body = [];
        text;
        nextStarPoint;
        nextStarPointDeltas = { X: 0, Y: 0 };
        hits = {};
        slicesGroup;
        slices = [];
        newSlices = null;
        newRects = null;
    }

    return {

        create: function () {
            //set background image
            //var background = game.add.sprite(0, 0, 'background');

            game.physics.setBoundsToWorld()
            initStage();

            //for debuggin purposes we draw the slices and bounding rects
            leftSlice = game.add.bitmapData(1600, 1000);
            var color = 'white';
            leftSlice.ctx.beginPath();
            leftSlice.ctx.lineWidth = "4";
            leftSlice.ctx.strokeStyle = color;
            leftSlice.ctx.stroke();
            game.add.sprite(0, 0, leftSlice);

            bottomSlice = game.add.bitmapData(1600, 1000);
            var color = 'red';
            bottomSlice.ctx.beginPath();
            bottomSlice.ctx.lineWidth = "4";
            bottomSlice.ctx.strokeStyle = color;
            bottomSlice.ctx.stroke();
            game.add.sprite(0, 0, bottomSlice);


            middleSlice = game.add.bitmapData(1600, 1000);
            var color = 'red';
            middleSlice.ctx.beginPath();
            middleSlice.ctx.lineWidth = "4";
            middleSlice.ctx.strokeStyle = color;
            middleSlice.ctx.stroke();
            game.add.sprite(0, 0, middleSlice);

            rightSlice = game.add.bitmapData(1600, 1000);
            var color = 'blue';
            rightSlice.ctx.beginPath();
            rightSlice.ctx.lineWidth = "4";
            rightSlice.ctx.strokeStyle = color;
            rightSlice.ctx.stroke();
            game.add.sprite(0, 0, rightSlice);

            boundingRect = game.add.bitmapData(1600, 1000);
            var color = 'green';
            boundingRect.ctx.beginPath();
            boundingRect.ctx.lineWidth = "4";
            boundingRect.ctx.strokeStyle = color;
            boundingRect.ctx.stroke();
            game.add.sprite(0, 0, boundingRect);

            innerRect = game.add.bitmapData(1600, 1000);
            var color = 'yellow';
            innerRect.ctx.beginPath();
            innerRect.ctx.lineWidth = "4";
            innerRect.ctx.strokeStyle = color;
            innerRect.ctx.stroke();
            game.add.sprite(0, 0, innerRect);

            outterRect = game.add.bitmapData(1600, 1000);
            var color = 'gray';
            outterRect.ctx.beginPath();
            outterRect.ctx.lineWidth = "4";
            outterRect.ctx.strokeStyle = color;
            outterRect.ctx.stroke();
            game.add.sprite(0, 0, outterRect);


            //set background image
            // var background = game.add.sprite(0, 0, 'background');

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

            //var oldBodyGroup = game.add.group();
            //for (var i = 0; i < 25; i++) {
            //    oldBodyGroup[i] = skeletonGroup.create(0, 0, 'dot');
            //}

            //slicesGroup = game.add.group();
            //slicesGroup.enableBody = true;
            //for (var i = 0; i < 4; i++) {
            //    slices[i] = slicesGroup.create(0, 0, 'a1');
            //    slices[i].scale.setTo(2.0, 2.0);
            //}

            //for (var i = 4; i < 9; i++) {
            //    slices[i] = slicesGroup.create(0, 0, 'a2');
            //    slices[i].scale.setTo(2.0, 2.0);
            //}
            //for (var i = 9; i < 12; i++) {
            //    slices[i] = slicesGroup.create(0, 0, 'a3');
            //    slices[i].scale.setTo(2.0, 2.0);
            //}
            text = game.add.text(10, 10, 'Good Luck!', { font: '30px Courier', fill: '#ffffff' });
            probText = game.add.text(10, 80, 'No guesses yet', { font: '30px Courier', fill: '#ffffff' });
            
            Server.GetInstance().RegisterForMessages(Server.Protocol.DISP_PROB, printProb);
            Server.GetInstance().RegisterForMessages(Server.Protocol.SKELETON_DATA, updateJoints);
            Server.GetInstance().RegisterForMessages(Server.Protocol.GET_NEXT_STAR_POSITION, drawNewStar);

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

            if (currentStar != undefined && currentStar.body) {                                     //Update the position of a moving object only if it is in the game area
                currentStar.body.position.x = nextStarPoint.X;
                currentStar.body.position.y = nextStarPoint.Y;
                //currentStar.scale.setTo(radius/(currentStar.body.width/2) , radius/(currentStar.body.height/2));
                console.log("Width scaling: " + radius / (currentStar.body.width / 2));
            }

            if (GlobalConfiguration.DebugMode) {

                if (newRects != null) {

                    var s = newRects;

                    boundingRect.clear();
                    boundingRect.color = 'green';
                    boundingRect.ctx.beginPath();
                    boundingRect.ctx.beginPath();
                    boundingRect.ctx.moveTo(s[0][0].X, s[0][0].Y);
                    boundingRect.ctx.lineTo(s[0][1].X, s[0][1].Y);
                    boundingRect.ctx.lineTo(s[0][2].X, s[0][2].Y);
                    boundingRect.ctx.lineTo(s[0][3].X, s[0][3].Y);
                    boundingRect.ctx.lineTo(s[0][0].X, s[0][0].Y);
                    boundingRect.ctx.lineWidth = 4;
                    boundingRect.ctx.stroke();

                    boundingRect.render();

                    innerRect.clear();
                    innerRect.color = 'yellow';
                    innerRect.ctx.beginPath();
                    innerRect.ctx.beginPath();
                    innerRect.ctx.moveTo(s[1][0].X, s[1][0].Y);
                    innerRect.ctx.lineTo(s[1][1].X, s[1][1].Y);
                    innerRect.ctx.lineTo(s[1][2].X, s[1][2].Y);
                    innerRect.ctx.lineTo(s[1][3].X, s[1][3].Y);
                    innerRect.ctx.lineTo(s[1][0].X, s[1][0].Y);
                    innerRect.ctx.lineWidth = 4;
                    innerRect.ctx.stroke();

                    innerRect.render();

                    outterRect.clear();
                    outterRect.color = 'gray';
                    outterRect.ctx.beginPath();
                    outterRect.ctx.beginPath();
                    outterRect.ctx.moveTo(s[2][0].X, s[2][0].Y);
                    outterRect.ctx.lineTo(s[2][1].X, s[2][1].Y);
                    outterRect.ctx.lineTo(s[2][2].X, s[2][2].Y);
                    outterRect.ctx.lineTo(s[2][3].X, s[2][3].Y);
                    outterRect.ctx.lineTo(s[2][0].X, s[2][0].Y);
                    outterRect.ctx.lineWidth = 4;
                    outterRect.ctx.stroke();

                    outterRect.render();

                    newRects = null;
                }

                if (newSlices != null) {
                    var s = newSlices;

                    leftSlice.clear();
                    leftSlice.color = 'white';
                    leftSlice.ctx.beginPath();
                    leftSlice.ctx.beginPath();
                    leftSlice.ctx.moveTo(s[0][0].X, s[0][0].Y);
                    leftSlice.ctx.lineTo(s[0][1].X, s[0][1].Y);
                    leftSlice.ctx.lineTo(s[0][2].X, s[0][2].Y);
                    leftSlice.ctx.lineTo(s[0][3].X, s[0][3].Y);
                    leftSlice.ctx.lineTo(s[0][0].X, s[0][0].Y);
                    leftSlice.ctx.lineWidth = 4;
                    leftSlice.ctx.stroke();


                    bottomSlice.clear();
                    bottomSlice.color = 'red';
                    bottomSlice.ctx.beginPath();
                    bottomSlice.ctx.beginPath();
                    bottomSlice.ctx.moveTo(s[3][0].X, s[3][0].Y);
                    bottomSlice.ctx.lineTo(s[3][1].X, s[3][1].Y);
                    bottomSlice.ctx.lineTo(s[3][2].X, s[3][2].Y);
                    bottomSlice.ctx.lineTo(s[3][3].X, s[3][3].Y);
                    bottomSlice.ctx.lineTo(s[3][0].X, s[3][0].Y);
                    bottomSlice.ctx.lineWidth = 4;
                    bottomSlice.ctx.stroke();


                    middleSlice.clear();
                    middleSlice.color = 'red';
                    middleSlice.ctx.beginPath();
                    middleSlice.ctx.beginPath();
                    middleSlice.ctx.moveTo(s[1][0].X, s[1][0].Y);
                    middleSlice.ctx.lineTo(s[1][1].X, s[1][1].Y);
                    middleSlice.ctx.lineTo(s[1][2].X, s[1][2].Y);
                    middleSlice.ctx.lineTo(s[1][3].X, s[1][3].Y);
                    middleSlice.ctx.lineTo(s[1][0].X, s[1][0].Y);
                    middleSlice.ctx.lineWidth = 4;
                    middleSlice.ctx.stroke();


                    rightSlice.clear();
                    rightSlice.color = 'blue';
                    rightSlice.ctx.beginPath();
                    rightSlice.ctx.beginPath();
                    rightSlice.ctx.moveTo(s[2][0].X, s[2][0].Y);
                    rightSlice.ctx.lineTo(s[2][1].X, s[2][1].Y);
                    rightSlice.ctx.lineTo(s[2][2].X, s[2][2].Y);
                    rightSlice.ctx.lineTo(s[2][3].X, s[2][3].Y);
                    rightSlice.ctx.lineTo(s[2][0].X, s[2][0].Y);
                    rightSlice.ctx.lineWidth = 4;
                    rightSlice.ctx.stroke();

                    newSlices = null;
                }
            }

        }
    }
})();