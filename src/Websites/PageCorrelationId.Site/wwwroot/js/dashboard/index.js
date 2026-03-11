$(function () {
    'use strict';

    loadStats();
    $('#btn-load-stats').on('click', loadStats);

    function loadStats() {
        var $result = $('#result');

        $result.text('Chargement…');

        $.ajax({
            url: '/Dashboard/GetStats',
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
