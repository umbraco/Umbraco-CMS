// Umbraco SpeechBubble Javascript

// Dependency Loader Constructor
function UmbracoSpeechBubble(id) {
    this.id = id;
    this.ie = document.all ? true : false;

    this.GenerateSpeechBubble();
}

UmbracoSpeechBubble.prototype.GenerateSpeechBubble = function () {

    var sbHtml = document.getElementById(this.id);

    sbHtml.innerHTML = '' +
        '<div class="speechBubbleTop"></div>' +
        '<div class="speechBubbleContent">' +
        '<img id="' + this.id + 'Icon" style="float: left; margin: 0px 5px 10px 3px;" />' +
        '                      <img class="speechClose" onClick="UmbSpeechBubble.Hide();" id="' + this.id + 'close" src="/umbraco/images/speechBubble/speechBubble_close.gif" width="18" height="18" border="0" alt="Close"' +
        '                        onmouseover="this.src = \'/umbraco/images/speechBubble/speechBubble_close_over.gif\';" onmouseout="this.src=\'images/speechBubble/speechBubble_close.gif\';">' +
        '                  <div style="float: right; width: 186px; margin-right: 10px;"><h3 id="' + this.id + 'Header">The header!</h3>' +
        '                  <p style="width: 185px" id="' + this.id + 'Message">Default Text Container!<br /></p></div><br style="clear: both" />' +
        '</div>' +
        '<div class="speechBubbleBottom"></div>';
};

UmbracoSpeechBubble.prototype.ShowMessage = function (icon, header, message, dontAutoHide) {
    var speechBubble = jQuery("#" + this.id);
    jQuery("#" + this.id + "Header").html(header);
    jQuery("#" + this.id + "Message").html(message);
    jQuery("#" + this.id + "Icon").attr('src', 'images/speechBubble/' + icon + '.png');

    if (!this.ie) {
        if (!dontAutoHide) {
            jQuery("#" + this.id).fadeIn("slow").animate({ opacity: 1.0 }, 5000).fadeOut("fast");
        } else {
            jQuery(".speechClose").show();
            jQuery("#" + this.id).fadeIn("slow");
        }
    } else {
        // this is special for IE as it handles fades with pngs very ugly
        jQuery("#" + this.id).show();
        if (!dontAutoHide) {
            setTimeout('UmbSpeechBubble.Hide();', 5000);
        } else {
            jQuery(".speechClose").show();
        }
    }
};

UmbracoSpeechBubble.prototype.Hide = function () {
    if (!this.ie) {
        jQuery("#" + this.id).fadeOut("slow");
    } else {
        jQuery("#" + this.id).hide();
    }
};

// Initialize
var UmbSpeechBubble = null
function InitUmbracoSpeechBubble() {
    if (UmbSpeechBubble == null)
        UmbSpeechBubble = new UmbracoSpeechBubble("defaultSpeechbubble");
}

jQuery(document).ready(function() {
    InitUmbracoSpeechBubble();
});
