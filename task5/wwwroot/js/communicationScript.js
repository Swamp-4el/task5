import * as signalr from '../js/microsoft/signalr/dist/browser/signalr.min.js';

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/messenger")
    .build();

hubConnection.on("GetMessages", function (messages) {
    blockFunctionality(false);
    displayingMessages(messages);
});

hubConnection.on("Disconnect", function () {
    document.getElementById("recipientUser").value = "";
    document.getElementById("title").value = "";
    document.getElementById("message").value = "";
    blockFunctionality(true);
    clearMessages();
});

hubConnection.on("AddUser", function (userName) {
    let userList = document.getElementById("users");
    let user = addUserBlockToUserList(userName);
    userList.appendChild(user);
});

hubConnection.on("NewMessage", function (message) {
    zeroingWhenMessageReceived();

    let userName = document.getElementById("userName").value;
    let recipientUser = document.getElementById("recipientUser").value;

    if (userName === message.senderId && recipientUser === message.recipientId ||
        userName === message.recipientId && recipientUser === message.senderId ||
        recipientUser === "" && userName === message.recipientId ||
        recipientUser === "" && userName === message.senderId) {
        addMessage(message);
    }
});

hubConnection.on("InvalidRecipientName", function () {
    document.getElementById("wrongName").style.display = "block";
});

document.addEventListener("DOMContentLoaded", () => {
    blockFunctionality(true);
    document.getElementById("wrongName").style.display = "none";
    document.getElementById("emptyFields").style.display = "none";
});

document.getElementById("connect").addEventListener("click", function (e) {
    let userName = document.getElementById("userName").value;
    hubConnection.invoke("ConnectUser", userName);
});

document.getElementById("send").addEventListener("click", function (e) {
    document.getElementById("wrongName").style.display = "none";
    document.getElementById("emptyFields").style.display = "none";

    let title = document.getElementById("title").value;
    let data = document.getElementById("message").value;

    if (!title && !data) {
        document.getElementById("emptyFields").style.display = "block";
        return;
    }

    hubConnection.invoke("Send", getMessage());
});

document.getElementById("disconnect").addEventListener("click", function (e) {
    hubConnection.invoke("DisconnectUser");
});

document.getElementById("recipientUser").addEventListener("change", function (e) {
    clearMessages();

    let userName = document.getElementById("userName").value;
    let recipientUser = document.getElementById("recipientUser").value;

    if (!recipientUser) {
        hubConnection.invoke("GetMessages", userName);
    }

    hubConnection.invoke("GetMessagesBetweenUsers", userName, recipientUser);
});

function addUserBlockToUserList(userName) {
    let user = document.createElement("option");
    user.className = "other-user";
    user.value = userName;
    user.appendChild(document.createTextNode(userName));

    return user;
}

function zeroingWhenMessageReceived(){
    document.getElementById("wrongName").style.display = "none";
    document.getElementById("emptyFields").style.display = "none";
    document.getElementById("title").value = "";
    document.getElementById("message").value = "";
}

function getMessage() {
    let recipient = document.getElementById("recipientUser").value;
    let title = document.getElementById("title").value;
    let data = document.getElementById("message").value;
    let user = document.getElementById("userName").value;

    return { "recipientId": recipient, "title": title, "data": data, "senderId": user };
}

function clearMessages() {
    var countMessages = document.querySelectorAll(".msg").length;
    for (let i = 0; i < countMessages; ++i) {
        document.querySelector(".msg").remove();
    }
}

function displayingMessages(messages) {
    for (let i = 0; i < messages.length; ++i) {
        addMessage(messages[i]);
    }
}

function addMessage(message) {
    let messagesBlock = document.getElementById("messages");
    let messageBlock = createMessageBlock(message);
    let firstMsg = document.querySelector(".msg");
    if (firstMsg)
        messagesBlock.insertBefore(messageBlock, firstMsg);
    else
        messagesBlock.appendChild(messageBlock);
}

function createBlockData(message) {
    let data = document.createElement("p");
    data.className = "form-text";
    data.appendChild(document.createTextNode(message.data));

    return data;
}

function createMessageBlock(message) {
    let messageBlock = document.createElement("div");
    messageBlock.className = "msg";

    let user = createBlockUser(message);
    let title = createBlockTitle(message);
    let date = createBlockDate(message);
    let data = createBlockData(message);

    messageBlock.appendChild(user);
    messageBlock.appendChild(title);
    messageBlock.appendChild(date);
    messageBlock.appendChild(data);

    return messageBlock;
}

function createBlockDate(message) {
    let date = document.createElement('p');
    date.className = 'form-check-label text-sm-left small';
    let u = document.createElement('u');
    u.appendChild(document.createTextNode(message.time));
    date.appendChild(u);

    return date;
}

function createBlockTitle(message) {
    let title = document.createElement('p');
    title.className = 'form-check-label';
    let b = document.createElement('b');
    b.appendChild(document.createTextNode(message.title));
    title.appendChild(b);

    return title;
}

function createBlockUser(message) {
    let userName = document.getElementById("userName").value;
    let user = document.createElement('p');
    let text;
    if (userName === message.senderId) {
        user.className = 'form-check-label p-2 bg-primary text-light';
    }
    else {
        user.className = 'form-check-label p-2 bg-secondary text-light';
    }
    text = `${message.senderId} -> ${message.recipientId}`;
    user.appendChild(document.createTextNode(text));

    return user;
}

function blockFunctionality(isBlocked) {
    document.getElementById("connect").disabled = !isBlocked;
    document.getElementById("userName").disabled = !isBlocked;
    document.getElementById("disconnect").disabled = isBlocked;
    document.getElementById("recipientUser").disabled = isBlocked;
    document.getElementById("title").disabled = isBlocked;
    document.getElementById("message").disabled = isBlocked;
    document.getElementById("send").disabled = isBlocked;
}

hubConnection.start();