function pickfield(select) {
    var control = jQuery(select);
    
    if (control.val() == "__setValue") {
        jQuery("input", control.parent()).show();
        jQuery("select", control.parent()).hide();
    }
}