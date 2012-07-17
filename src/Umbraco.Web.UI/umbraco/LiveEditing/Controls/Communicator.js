// Umbraco Live Editing: Communicator

/********************* Communicator Constructor *********************/

    function UmbracoCommunicator() { }



/********************* Communicator Methods *********************/

    // Sends a message to the client using the communicator.
    UmbracoCommunicator.prototype.SendClientMessage = function(type, message) {
        // find the communicator
        var divs = document.getElementsByTagName("div");
        var communicator = null;
        for (var i = 0; i < divs.length && communicator == null; i++)
            if (divs[i].className == "communicator")
            communicator = divs[i];
        Sys.Debug.assert(communicator != null, "LiveEditing: Communicator not found.");

        // send the message
        var typeBox = communicator.childNodes[0].childNodes[0];
        var messageBox = communicator.childNodes[0].childNodes[1];
        var submit = communicator.childNodes[0].childNodes[2];
        typeBox.value = type;
        messageBox.value = message;
        submit.click();
    }


/********************* Communicator Instance *********************/

    var UmbracoCommunicator = new UmbracoCommunicator();