var loadState = (function () {
   return {
        preload: function () {
            var loadingLabel = game.add.text(80, 150, 'loading...', { font: '30px Courier', fill: '#ffffff' });
            game.load.image('background', 'assets/background4.jpg');
            game.load.image('sword', 'assets/sword.png');
            game.load.image('a1', 'assets/asteroid1.png');
            game.load.image('a2', 'assets/asteroid2.png');
            game.load.image('a3', 'assets/asteroid3.png');
            game.load.image('a4', 'assets/asteroid4.png');
            game.load.image('a5', 'assets/asteroid5.png');
            game.load.image('particle', 'assets/particle.png')
            game.load.image('r1', 'assets/rocket1.png');
            game.load.image('r2', 'assets/rocket2.png');
            game.load.image('dot', 'assets/dot.png');
        },
        create: function () {
            game.state.start('instructions');
        }
    };
})();

