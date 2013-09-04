myDesignSurface.addEventListener("ShowAddFieldDialog", "showaddfielddialog", function() {
    $("#fieldtype").msDropDown();
});

myDesignSurface.addEventListener("ShowUpdateFieldDialog", "showupdatefielddialog", function() {
    $("#fieldtype").msDropDown();
});

myDesignSurface.addEventListener("AddPage", "dirtyaddpage", function(id,name) {
    setDirty(true);
});

myDesignSurface.addEventListener("DeletePage", "dirtydeletepage", function(id) {
    setDirty(true);
});

myDesignSurface.addEventListener("UpdatePage", "dirtyupdatepage", function(id, name) {
    setDirty(true);
});

myDesignSurface.addEventListener("UpdatePageSortOrder", "dirtyupdatepagesortorder", function(sortorder) {
    setDirty(true);
});

myDesignSurface.addEventListener("AddFieldset", "dirtyaddfieldset", function(pageid, id, name) {
    setDirty(true);
});

myDesignSurface.addEventListener("DeleteFieldset", "dirtydeletefieldset", function(id) {
    setDirty(true);
});

myDesignSurface.addEventListener("UpdateFieldset", "dirtyupdatefieldset", function(id,name) {
    setDirty(true);
});

myDesignSurface.addEventListener("UpdateFieldsetSortOrder", "dirtyupdatefieldsetsortorder", function(id,sortorder) {
    setDirty(true);
});


myDesignSurface.addEventListener("AddField", "dirtyaddfield", function(fieldsetid,id,name,type,mandatory,regex) {
    setDirty(true);
});

myDesignSurface.addEventListener("DeleteField", "dirtydeletefield", function(id) {
    setDirty(true);
});

myDesignSurface.addEventListener("UpdateField", "dirtyupdatefield", function(id,name,type,mandatory,regex) {
    setDirty(true);
});


myDesignSurface.addEventListener("UpdateFieldSortOrder", "dirtyupdatefieldsortorder", function(id, sortorder) {
    setDirty();
});


