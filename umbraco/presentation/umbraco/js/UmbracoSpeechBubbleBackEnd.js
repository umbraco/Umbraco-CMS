// Umbraco SpeechBubble Javascript

// Dependency Loader Constructor
function UmbracoSpeechBubble(id) {
    this.id = id;
    this.ie = document.all ? true : false;

    this.GenerateSpeechBubble();
}

UmbracoSpeechBubble.prototype.GenerateSpeechBubble = function() {

    var sbHtml = document.getElementById(this.id);

    sbHtml.innerHTML = '' +
	        '<div id="' + this.id + 'Icon" style="left: 10px; position: absolute; top: 16px; width: 30px; height: 30px; background: no-repeat center center url(images/speechbubble/speechBubble/info.png);">&nbsp;' +
            '</div>' +
            '    <div id="speechClose" style="left: 208px; position: absolute; top: 6px">' +
            '          <a href="javascript:UmbSpeechBubble.Hide()">' +
            '                      <img id="' + this.id + 'close" style="display: none;" src="/umbraco/images/speechBubble/speechBubble_close.gif" width="18" height="18" border="0" alt="Close"' +
            '                        onmouseover="this.src = \'/umbraco/images/speechBubble/speechBubble_close_over.gif\';" onmouseout="this.src=\'images/speechBubble/speechBubble_close.gif\';"></a></div>' +
            '                  <div id="' + this.id + 'Header" style="font-family: Segoe UI, Trebuchet MS, Lucida Grande, verdana, arial; font-size: 16px; font-weight: 100; color: #0033aa; left: 50px;' +
            '                    position: absolute; top: 6px">' +
            '                    Data gemt!</div>' +
            '                  <div id="' + this.id + 'Message" style="font-family: Segoe UI, Trebuchet MS, Lucida Grande, verdana, arial; font-size: 11px; font-weight: normal; color: #000; text-align: left; left: 50px; width: 180px; position: absolute;' +
            '                    top: 28px">' +
            '                    Default Text Container!</div>' +
            '                </div>';
}

UmbracoSpeechBubble.prototype.ShowMessage = function(icon, header, message, dontAutoHide) {
    var speechBubble = document.getElementById(this.id);
    document.getElementById(this.id + "Header").innerHTML = header;
    document.getElementById(this.id + "Message").innerHTML = message;
    document.getElementById(this.id + "Icon").style.backgroundImage = "url('images/speechBubble/" + icon + ".png')";

    speechBubble.style.right = "20px";
    speechBubble.style.bottom = "20px";

    if (!dontAutoHide) {
        jQuery("#" + this.id).fadeIn("slow").animate({ opacity: 1.0 }, 5000).fadeOut("fast");
    } else {
        jQuery("#" + this.id + "close").show();
        jQuery("#" + this.id).fadeIn("slow");
    }
}

UmbracoSpeechBubble.prototype.Hide = function() {
    jQuery("#" + this.id).fadeOut("slow");
}

// Initialize
var UmbSpeechBubble = null
function InitUmbracoSpeechBubble() {
    if (UmbSpeechBubble == null)
        UmbSpeechBubble = new UmbracoSpeechBubble("defaultSpeechbubble");
}

jQuery(document).ready(function() {
    InitUmbracoSpeechBubble();
});