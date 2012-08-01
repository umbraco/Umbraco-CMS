//var modalActive = false;

//var rightWindowTab = false;
//var rightButtons = false;

//var nodeID = -1;
//var nodeKey = '';
//var nodeType = '';
//var nodeName = '';
//var topNodeID = -1;
//var currentApp = "";
//var nodeDeleteEffectElement = null;
//var deleteNode;

///* REFRESH NODE CODE */

//var expandTries = 0;

//var refresh = true;
//var refreshParent = true;
//var dontDelete = false;

//function deleteEffect() {
//    if (deleteNode.parentNode.childNodes.length == 1)
//        deleteNode.parentNode.childNodes = [];

//    // Add delete puff effectEffects
//    deleteNode.collapse();
//    nodeDeleteEffectElement = deleteNode;
//    tree.jQuery("#" + deleteNode.id).hide("drop", { direction: "down" }, 400);

//    // Update recyclebin
//    if ((currentApp == "" || currentApp == "content") && deleteNode.parentNode.nodeID != -20)
//        updateRecycleBin();

//    setTimeout('nodeDeleteEffectElement.remove(); tree.jQuery("#fxWrapper").hide();', 1000);
//}

//function refreshTree(refParent, dontDel) {

//    expandTries = 0;
//    refreshParent = refParent;
//    dontDelete = dontDel;

//    if (refreshParent) {

//        if (node != null && refresh) {
//            if (node.parentNode != null) {
//                if (dontDelete)
//                    node = node.parentNode;
//                else {
//                    if (node.parentNode.childNodes.length == 1)
//                        node.parentNode.childNodes = [];

//                    // Add delete puff effectEffects
//                    node.collapse();
//                    new tree.Effect.DropOut(node.id);
//                    nodeDeleteEffectElement = node;

//                    // Update recyclebin
//                    if (node.parentNode.nodeID != -20)
//                        updateRecycleBin();

//                    setTimeout('nodeDeleteEffectElement.remove()', 1000);
//                }
//            }
//        }
//    }

//    if (!refreshParent || (refreshParent && dontDelete)) {
//        if (node != null && refresh) {
//            if (node.parentNode != null) {
//                if (node.src != "" && node.src != null) {
//                    node.src = node.src + '&rnd=' + returnRandom();
//                    node.reload();
//                    setTimeout("expandNode()", 200);
//                } else if (node.srcRoot != "" && node.srcRoot != null) {
//                    node.src = node.srcRoot;
//                    node.reload();
//                    node.expand();
//                    setTimeout("expandNode()", 200);
//                }
//            } else {
//                tree.document.location.href = tree.document.location.href;
//            }
//        } else if (!refresh)
//            tree.document.location.href = tree.document.location.href;
//    }
//}

//function fetchUrl(url) {
//    url = url.split("\/");
//    if (url.length > 0)
//        tmp = url[url.length - 1];
//    else
//        tmp = url;

//    if (tmp.indexOf("?") > 0)
//        tmp = tmp.substring(0, tmp.indexOf("?"));
//    return tmp;
//}

//function expandNode() {

//    if (parent.node.childNodes.length == 0 && expandTries < 10) {
//        expandTries++;
//        setTimeout("expandNode()", 200);
//    } else {
//        parent.node.expand();
//        expandTries = 0;
//    }
//}

//function reloadParentNode(expand) {

//    var reloadNode = false;

//    if (node != null)
//        if (node.parentNode)
//        if (node.parentNode.parentNode)
//        reloadNode = true;

//    if (reloadNode) {
//        node.parentNode.src = node.parentNode.src + "&rnd2=" + returnRandom();
//        node.parentNode.reload();

//        if (expand)
//            node.parentNode.expand();

//    } else
//        tree.document.location.href = tree.document.location.href;


//}

//function reloadCurrentNode() {

//    var reloadNode = false;

//    if (node != null)
//        if (node.parentNode)
//        reloadNode = true;

//    if (reloadNode) {
//        node.src = node.src + "&rnd2=" + returnRandom();
//        node.reload();
//        node.expand();
//    } else
//        tree.document.location.href = tree.document.location.href;
//}



//function returnRandom() {
//    day = new Date()
//    z = day.getTime()
//    y = (z - (parseInt(z / 1000, 10) * 1000)) / 10
//    return y
//}

//function disableModal() {
//    modalActive = false;
//}


/*
function openModal(url, name, mheight, mwidth) {
top.focus();
    
Modalbox.show(url, { title: name, iframe: true, height: mheight, width: mwidth });
return false;
}
*/


//var theDialogWindow = null;

//function openDialog(diaTitle, diaDoc, dwidth, dheight, optionalParams) {
//    modalActive = true;

//    if (theDialogWindow != null && !theDialogWindow.closed) {
//        theDialogWindow.close();
//    }
//    theDialogWindow = window.open(diaDoc, 'dialogpage', "width=" + dwidth + "px,height=" + dheight + "px" + optionalParams); // window.showModalDialog(diaDoc, "MyDialog", strFeatures);
//}


//function createNew() {
//    if (nodeType != '') {
//        if (currentApp == "content" || currentApp == "")
//            openModal("create.aspx?nodeID=" + nodeID + "&nodeType=" + nodeType + "&nodeName=" + nodeName + '&rnd=' + returnRandom(), uiKeys['actions_create'], 425, 600);
//        else if (nodeType == "initmember") {
//            openModal("create.aspx?nodeID=" + nodeID + "&nodeType=" + nodeType + "&nodeName=" + nodeName + '&rnd=' + returnRandom(), uiKeys['actions_create'], 380, 420);
//    } else
//            openModal("create.aspx?nodeID=" + nodeID + "&nodeType=" + nodeType + "&nodeName=" + nodeName + '&rnd=' + returnRandom(), uiKeys['actions_create'], 270, 420);

//        return false;

//    }
//}

//function createFolder() {
//    if (nodeType != '') {
//        openDialog("Opret", "createFolder.aspx?nodeID=" + nodeID + "&nodeType=" + nodeType + "&nodeName=" + nodeName + '&rnd=' + returnRandom(), 320, 225);

//    }
//}

//function importDocumentType() {
//    if (nodeType != '') {
//        openModal("dialogs/importDocumentType.aspx?rnd=" + returnRandom(), uiKeys['actions_importDocumentType'], 460, 400);
//        return false;
//    }
//}

//function exportDocumentType() {
//    if (nodeType != '') {
//        openDialog("Export", "dialogs/exportDocumentType.aspx?nodeID=" + nodeID + "&rnd=" + returnRandom(), 320, 205);

//    }
//}

//function refreshNode() {

//    if (nodeKey != '') {
//        if (tree.webFXTreeHandler.all[nodeKey]) {
//            tree.webFXTreeHandler.all[nodeKey].src =
//				tree.webFXTreeHandler.all[nodeKey].src + '&rnd=' + Math.random() * 10;

//            if (tree.webFXTreeHandler.all[nodeKey].parentNode) {
//                // Hvis punktet er lukket, skal det åbnes
//                if (tree.webFXTreeHandler.all[nodeKey].childNodes.length > 0) {
//                    if (!tree.webFXTreeHandler.all[nodeKey].open)
//                        tree.webFXTreeHandler.all[nodeKey].expand();
//                    tree.webFXTreeHandler.all[nodeKey].reload();
//                    treeEdited = true;
//                }
//            } else {
//                tree.document.location.href = tree.document.location.href;
//            }
//        }
//    }
//}

//////NEVER USED!!!!!! OLD CODE
//////function accessThis() {
//////    if (nodeID != '-1' && nodeType != '') {

//////        task.document.dataForm.nodeID.value = nodeID;
//////        task.document.dataForm.nodeType.value = nodeType;

//////        newName = window.open("dialogs/publicAccess.aspx?nodeID=" + nodeID + '&rnd=' + returnRandom(), "access", 'width=530,height=550,scrollbars=auto');

//////    }
//////}


//function assignDomain() {
//    if (nodeID != '-1' && nodeType != '') {

//        openModal("dialogs/assignDomain.aspx?id=" + nodeID, uiKeys['actions_assignDomain'], 420, 500);
//        return false;

//        //		newName = window.open("dialogs/assignDomain.aspx?id="+nodeID, "assignDomain", 'width=500,height=450,scrollbars=yes');

//    }
//}

//function publish() {
//    if (nodeID != '-1' && nodeType != '') {

//        openModal("dialogs/publish.aspx?id=" + nodeID, uiKeys['actions_publish'], 280, 540);
//        return false;

//        //newName = window.open("dialogs/publish.aspx?id="+nodeID, "publish", 'width=500,height=250,scrollbars=auto');

//    }
//}

//function about() {
//    openModal("dialogs/about.aspx", uiKeys['general_about'], 390, 450);
//    return false;
//}


//function emptyTrashcan() {
//    if (nodeID != '-1' && nodeType != '') {

//        openModal("dialogs/emptyTrashcan.aspx", uiKeys['actions_emptyTrashcan'], 220, 500);
//        return false;

//    }
//}

//function sortThis() {
//    if (nodeID != '0' && nodeType != '') {
//        // task.document.dataForm.nodeID.value = nodeID;
//        // task.document.dataForm.nodeType.value = nodeType;
//        //openDialog("Sort", "sort.aspx?id="+nodeID + '&app=' + currentApp + '&rnd='+returnRandom(), 600, 450,',scrollbars=yes');
//        /*
//        // læg variable i form
//        if (newName != 'undefined' && newName != null) {
//        task.document.dataForm.task.value = "sort";
//        task.document.dataForm.parameterName.value = newName;
//        // submit form
//        task.document.dataForm.submit();
//        }
//        */

//        openModal("dialogs/sort.aspx?id=" + nodeID + '&app=' + currentApp + '&rnd=' + returnRandom(), uiKeys['actions_sort'], 450, 600);
//        return false;


//    }
//}

//function protectThis() {
//    if (nodeID != '-1' && nodeType != '') {
//        openModal("dialogs/protectPage.aspx?app=" + currentApp + "&mode=cut&nodeId=" + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_protect'], 480, 535);
//        return false;
//    }
//}

//function moveThis() {
//    if (nodeID != '-1' && nodeType != '') {

//        openModal("dialogs/moveOrCopy.aspx?app=" + currentApp + "&mode=cut&id=" + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_move'], 460, 500);
//        return false;
//    }
//}

//function rightsThis() {
//    if (nodeID != '-1' && nodeType != '') {

//        openModal("dialogs/cruds.aspx?id=" + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_rights'], 300, 800);
//        return false;
//    }
//}


////////NEVER USED. OLD CODE!
////////function showDashboard() {
////////    openModal("dashboard.aspx?app" + currentApp + "&modal=true", currentApp + uiKeys['actions_dashboard'], 600, 800);
////////    return false;
////////}

//function changeDashboard(path) {
//    right.location.href = path;
//}

//function notifyThis() {
//    if (nodeID != '-1' && nodeType != '') {

//        openModal("dialogs/notifications.aspx?id=" + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_notify'], 480, 300);
//        return false;
//        //newName = openDialog("notifications", "dialogs/modalHolder.aspx?url=notifications.aspx&params=id="+nodeID + '&rnd='+returnRandom(), 680, 340);
//    }
//}

//function createWizard() {
//    if (currentApp == 'media' || currentApp == 'content' || currentApp == '') {
//        if (currentApp == '') currentApp = 'content';

//        openModal("dialogs/create.aspx?nodeType=" + currentApp + "&app=" + currentApp + "&rnd=" + returnRandom(), uiKeys['actions_create'] + " " + currentApp, 470, 620);
//        return false;

//        //openDialog("create", "dialogs/create.aspx?nodeType=" + currentApp + "&app=" + currentApp + "&rnd="+returnRandom(), 600, 470);

//    } else
//        alert('Not supported - please create by right clicking the parentnode and choose new...');
//}

//function copyThis() {
//    if (nodeID != '-1' && nodeType != '') {
//        openModal("dialogs/moveOrCopy.aspx?app=" + currentApp + "&mode=copy&id=" + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_copy'], 470, 500);
//        return false;
//    }
//}

//function translateThis() {
//    if (nodeID != '-1' && nodeType != '') {

//        openModal("dialogs/sendToTranslation.aspx?app=" + currentApp + "&id=" + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_sendToTranslate'], 470, 500);
//        return false;

//        //newName = openDialog("translate", "dialogs/modalHolder.aspx?url=sendToTranslation.aspx&params=app=" + currentApp + "|id="+nodeID + '&rnd='+returnRandom(), 500, 450);

//    }
//}

//function liveEdit() {
//    window.open("canvas.aspx?redir=/" + nodeID + ".aspx", "liveediting");
//}

////SD: Added so the send to publish context menu works
//function toPublish() {
//    if (nodeID != '-1' && nodeType != '') {
//        if (confirm(parent.uiKeys['defaultdialogs_confirmSure'] + '\n\n')) {
//            openModal('dialogs/SendPublish.aspx?id=' + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_sendtopublish'], 200, 300);
//            return false;
//        }
//    }
//}
/*
function deleteThis() {
var tempID = nodeID;
var tempNodeType = nodeType;
var tempNodeName = nodeName;
if (confirm(parent.uiKeys['defaultdialogs_confirmdelete'] + ' "' + nodeName + '"?\n\n')) {
deleteNode = node;
nodeID = tempID;
nodeType = tempNodeType;
nodeName = tempNodeName;
umbracoStartXmlRequest('webservices/aspx_ajax_calls/delete.aspx?nodeId=' + tempID + '&nodeType=' + tempNodeType + '&nodeName=' + nodeName, '', 'refreshDelete()');
}
}*/

//function deleteThis() {
//    var doDelete = true

//    if (currentApp == "content" && nodeID == '-1')
//        doDelete = false;

//    if (doDelete) {
//        var tempID = nodeID;
//        var tempNodeType = nodeType;
//        var tempNodeName = nodeName;

//        if (confirm(parent.uiKeys['defaultdialogs_confirmdelete'] + ' "' + nodeName + '"?\n\n')) {
//            deleteNode = node;
//            nodeID = tempID;
//            nodeType = tempNodeType;
//            nodeName = tempNodeName;
//            umbraco.presentation.webservices.legacyAjaxCalls.Delete(tempID, "", tempNodeType, refreshDelete);
//        }
//    }
//}

//function refreshDelete() {
//    deleteEffect();
//}

//// Just used for users, which aren't deleted but only disabled
//function disableThis() {
//    if (confirm(parent.uiKeys['defaultdialogs_confirmdisable'] + ' "' + nodeName + '"?\n\n')) {
//        umbraco.presentation.webservices.legacyAjaxCalls.DisableUser(nodeID, refreshDisabled);
//    }
//}

//function refreshDisabled() {
//    tree.document.location.href = tree.document.location.href + "&refresh=true";
//}

//function republish() {
//    openModal('dialogs/republish.aspx?rnd=' + returnRandom(), 'Republishing entire site', 210, 450);
//    return false;
//}

//function viewAuditTrail() {
//    openModal('dialogs/viewAuditTrail.aspx?nodeID=' + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_auditTrail'], 500, 550);
//    return false;
//}

//function rollback() {
//    openModal('dialogs/rollback.aspx?nodeID=' + nodeID + '&rnd=' + returnRandom(), uiKeys['actions_rollback'], 550, 600);
//    return false;
//}

/////////NEVER USED. OLD CODE!
//////////function importPackage() {
//////////    parent.openDialog('packager', 'dialogs/packager.aspx?rnd=' + parent.returnRandom(), 530, 550, ',scrollbars=yes');
//////////}


//function closeUmbraco() {
//    if (confirm(parent.uiKeys['defaultdialogs_confirmlogout'] + '\n\n'))
//        document.location.href = 'logout.aspx';
//}

//function shiftApp(whichApp, appName, ignoreDashboard) {
//    currentApp = whichApp.toLowerCase();

//    if (currentApp != 'media' && currentApp != 'content') {
//        document.getElementById("buttonCreate").disabled = true;
//    }
//    else {
//        document.getElementById("buttonCreate").disabled = false;
//    }
//    
//    if (!ignoreDashboard) {
//        window.frames["right"].location = 'dashboard.aspx?app=' + whichApp;
//    }

//    UmbClientMgr.mainTree().rebuildTree(whichApp);

//    top.document.getElementById("treeWindowLabel").innerHTML = appName;
//    top.document.title = appName + " - Umbraco CMS" + " - " + window.location.hostname.toLowerCase().replace('www', '');

//    top.document.location.hash = whichApp;
//}



/*
Synchonizes the tree to the document path (orgPath).
Since the last part of the path is the node ID that should be synchronized, attempt
to find the node with the last part of the path. If it is not found and the path length is
greater than 1, then attempt to synchronize it's parent recursively.
This method would be able to synchronize the tree with any number of node ids seperated by a comma.
For example, the value 1,2,444,666,2345
would first search for node 2345, and if not found, then 666, and if not found, then 444 and so on...
*/

//function syncTree(orgPath, isNew, newName, newPublishStatus) {
//    var path = orgPath.split(",");
//    tree.SyncTree(path, isNew, newName, newPublishStatus);
//}


//function findNodeById(id) {
//    return findNodeByIdDo(tree.tree, id);
//}

//function findNodeByIdDo(node, id) {
//    var childNodes = node.childNodes;

//    if (childNodes != null)
//        for (var i = 0; i < childNodes.length; i++) {
//        if (childNodes[i].nodeID == id)
//            return childNodes[i];
//        else if (childNodes[i].childNodes != null) {
//            var tempNode = findNodeByIdDo(childNodes[i], id);
//            if (tempNode != "")
//                return tempNode;
//        }
//    }

//    return "";
//}

//var recycleBin;
//function updateRecycleBin() {
//    recycleBin = findNodeById(-20);
//    tree.jQuery("#" + recycleBin.id).effect("highlight", {}, 1000);

//    if (recycleBin.src == "") {
//        recycleBin.src = "tree.aspx?isRecycleBin=true&isDialog=&dialogMode=&app=content&id=" + recycleBin.nodeID + "&treeType=" + currentApp.toLowerCase();
//        tree.document.getElementById(recycleBin.id + '-plus').src = recycleBin.plusIcon;
//        recycleBin.folder = true;
//        recycleBin.forceLoad(recycleBin.src, recycleBin);
//    } else {
//        recycleBin.src += "&rnd=" + Math.random() * 10;
//        recycleBin.reload();
//    }



//}
