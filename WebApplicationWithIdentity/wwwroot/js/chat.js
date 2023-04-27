"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/Chat").build();



connection.on("ReceiveMessage", function (user, message) {

    let div = document.createElement('div');

    div.classList.add('div-class');


    let userSpan = document.createElement('span');
    userSpan.classList.add('user-span');
    userSpan.textContent = `${user}`;

    div.appendChild(userSpan);


    let messageSpan = document.createElement('span');
    messageSpan.classList.add('message-span');
    messageSpan.textContent = `${message}`;

    div.appendChild(document.createElement('br'));
    div.appendChild(messageSpan);


    let timeSpan = document.createElement('span');
    timeSpan.classList.add('time-span');
    let now = new Date();
    let timeString = now.getHours().toString().padStart(2, '0') + ':' + now.getMinutes().toString().padStart(2, '0');
    timeSpan.textContent = timeString;

    div.appendChild(document.createElement('br'));
    div.appendChild(timeSpan);

    document.getElementById("messagesList").appendChild(div);

    //console.log("connection on");
    //var newDiv = document.createElement("div");
    //newDiv.classList.add('div-class');
    //newDiv.innerHTML = `${user} <br> ${message}`;
    //document.getElementById("messagesList").appendChild(newDiv);


});

connection.start().then(function () {
    console.log("connection start");
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    console.log("event button");
    var user = document.getElementById("userInput").value;
    console.log(typeof (user));
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});