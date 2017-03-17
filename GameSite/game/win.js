var winState = (function () {
    return {

        create: function () {

            //set background image
            var background = game.add.sprite(0, 0, 'background');
            text = game.add.text(200, game.world.height / 2, 'Thank you very very much!', { font: '72px Courier', fill: '#ffffff' });
            Server.GetInstance().Send({ Type: Server.Protocol.GAME_DONE });
        },
        update: function () {

        }
    }
})();