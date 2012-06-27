var activecolorpicker;

jQuery('input.skinningcolorpicker').ColorPicker({
    onSubmit: function (hsb, hex, rgb, el) {
        jQuery(el).val('#' + hex);
        jQuery(el).ColorPickerHide();
        jQuery(el).trigger('change');
    },
    onBeforeShow: function () {
        activecolorpicker = this;
        jQuery(this).ColorPickerSetColor(this.value);
    },
    onChange: function (hsb, hex, rgb) {
       
        jQuery(activecolorpicker).val('#' + hex);
        jQuery(activecolorpicker).trigger('change');
    }
})
.bind('keyup', function () {
    jQuery(this).ColorPickerSetColor(this.value);
});

