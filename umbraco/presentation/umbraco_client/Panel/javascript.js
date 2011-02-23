function resizePanel(PanelName, hasMenu, redoOnResize) {
    var clientHeight = jQuery(window).height();
    var clientWidth = jQuery(window).width();

    if (top.document.location == document.location)
        resizePanelTo(PanelName, hasMenu, clientWidth - 12, clientHeight - 20);
    else
        resizePanelTo(PanelName, hasMenu, clientWidth, clientHeight);

    if (redoOnResize) {
        jQuery(window).resize(function() {
            resizePanel(PanelName, hasMenu, false);
        });
    }
}


function resizePanelTo(PanelName, hasMenu, pWidth, pHeight) {

    var panel = jQuery("#" + PanelName);
    var panelContent = jQuery("#" + PanelName + "_content");

    panelWidth = pWidth;
    contentHeight = pHeight - 46;

    if (hasMenu) contentHeight = contentHeight - 34;

    panel.width(panelWidth);

    if (pHeight > 0)
        panel.height(pHeight);

    if (panelContent != null) {
        if (panelWidth > 0)
            panelContent.width(panelWidth - 6);
        if (contentHeight > 0)
            panelContent.height(contentHeight);
    }

    if (hasMenu && panelWidth > 0) {
        var scrollwidth = panelWidth - 35;
        jQuery("#" + PanelName + "_menu").width(scrollwidth);
        jQuery("#" + PanelName + "_menu_slh").width(scrollwidth);
        jQuery("#" + PanelName + "_menubackground").width(panelWidth - 2);
    }

    // set cookies
    jQuery.cookie('UMB_PANEL', '' + pWidth + 'x' + pHeight);
}
