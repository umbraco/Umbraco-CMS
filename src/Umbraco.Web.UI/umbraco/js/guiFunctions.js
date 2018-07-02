// ---------------------------------------------
// guiFunctions
// ---------------------------------------------
function toggleTree(sender) {
    var tree = jQuery("#leftDIV");
    var frame = jQuery("#rightDIV");
    
    var disp = tree.css("display")
    var link = jQuery(sender);
    

    if (disp == "none") {
        tree.show();
        link.removeClass();
        resizePage();
    }
    else {
        tree.hide();
        link.addClass("on");
        var clientWidth = jQuery(window).width();
        frame.width(clientWidth - 20);
    }
}

function resizePage(sender) {
    var dashboard = jQuery("#rightDIV");
    var dashboardFrame = jQuery("#right");
    var tree = jQuery("#leftDIV");
    var treeToggle = jQuery("#treeToggle");
    var appIcons = jQuery("#PlaceHolderAppIcons");
    var uiArea = jQuery("#uiArea");
    
    if (jQuery(window)) {
        var clientHeight = jQuery(window).height() - 48;
        var clientWidth = jQuery(window).width();
        var leftWidth = parseInt(clientWidth * 0.25);
        var rightWidth = parseInt(clientWidth - leftWidth - 30); 

        // check if appdock is present
        var treeHeight = parseInt(clientHeight - 5);

        // resize leftdiv
        tree.width(leftWidth);

        if (appIcons != null) {
            treeHeight = treeHeight - 135;
            resizeGuiWindow("PlaceHolderAppIcons", leftWidth, 140);
        }

        resizeGuiWindow("treeWindow", leftWidth, treeHeight)

        if (tree.css("display") == "none") {
            dashboard.width(clientWidth - 24);
        } else {
            dashboard.width(rightWidth);
        }
        if (clientHeight > 0) {
            dashboard.height(clientHeight);
            treeToggle.height(clientHeight);
        }

        treeToggle.show();
        uiArea.show();
    }
     
}

function resizeGuiWindow(windowName, newWidth, newHeight, window) {
    //This no longer does anything and shouldn't be used.
}

function resizeGuiWindowWithTabs(windowName, newWidth, newHeight) {
    right.document.all[windowName + "ContainerTable"].width = newWidth + 22
    right.document.all[windowName + "ContainerTableSpacer"].width = newWidth
    right.document.all[windowName + "Bottom"].width = newWidth + 12
    right.document.all[windowName + "BottomSpacer"].width = newWidth
    right.document.all[windowName].style.width = newWidth


    // Der skal forskellig størrelse på højden afhængig af om vinduet har en label i bunden
    if (right.document.all[windowName + 'BottomLabel']) {
        right.document.all[windowName + "ContainerTable"].height = newHeight - 13;
        right.document.all[windowName].style.height = newHeight - 13;
    } else {
        right.document.all[windowName + "ContainerTable"].height = newHeight + 3;
        right.document.all[windowName].style.height = newHeight + 3;
    }
}	
