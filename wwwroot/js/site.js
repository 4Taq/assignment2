// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    // Add to cart with confetti
    $(".add-to-cart").click(function () {
        var eventId = $(this).data("id");
        $.post("/Cart/Add", { eventId: eventId, qty: 1 }, function (res) {
            if (res.success) {
                // Show confetti modal
                $('#confettiModal').modal('show');
                particlesJS("confetti", {
                    "particles": { "number": { "value": 150 }, "color": { "value": "#ff0000" }, "shape": { "type": "circle" } }
                });
            }
        });
    });
});