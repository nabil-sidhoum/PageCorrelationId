$(function () {
    'use strict';

    loadSomething();
    $('#btn-load-something').on('click', loadSomething);

    function loadSomething() {
        var $result = $('#result');

        $result.text('Chargement…');

        $.ajax({
            url: '/Dashboard2/GetSomething',
            method: 'GET',
            success: function (data) {
                $result.text(JSON.stringify(data, null, 2));
            },
            error: function (xhr) {
                $result.text('Erreur HTTP ' + xhr.status + ' : ' + xhr.statusText);
            }
        });
    }

});
