//Boot state currently only loading the physics system
//create function is called automatically by phaser calling convention
var bootState= (function () {
    return {
        create: function () {
            
            //load the ARCADE physics system
            game.physics.startSystem(Phaser.Physics.ARCADE);

            console.log('boot.js done, moving to load state');
            //go to load state
            game.state.start('load');
            
        }
    };
})();
