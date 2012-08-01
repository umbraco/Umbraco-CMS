// Umbraco SpeechBubble Javascript

// Dependency Loader Constructor
function UmbracoSpeechBubble(id) {
    this.id = id;
    this.ie = document.all ? true : false;

    this.GenerateSpeechBubble();
}

UmbracoSpeechBubble.prototype.GenerateSpeechBubble = function() {

    theBody = document.getElementsByTagName('BODY')[0];
    sbHtml = document.createElement('div');
    sbHtml.id = this.id + 'Container';
    sbHtml.innerHTML = '' +
	    '<div id="' + this.id + '" style="z-index: 10; filter: Alpha(Opacity=0); background-image: url(/umbraco/images/speechbubble/speechbubble.gif); visibility: hidden; width: 231px; position: absolute; height: 84px">' +
	        '<div id="' + this.id + 'Icon" style="left: 10px; position: absolute; top: 16px">' +
            '<img src="/umbraco/images/speechBubble/info.gif" alt="Info" width="30" height="30" id="' + this.id + 'IconSrc"></div>' +
            '    <div id="speechClose" style="left: 208px; position: absolute; top: 6px">' +
            '          <a href="javascript:UmbSpeechBubble.Hide(100)">' +
            '                      <img src="/umbraco/images/speechbubble/speechBubble_close.gif" width="18" height="18" border="0" alt="Close"' +
            '                        onmouseover="this.src = \'/umbraco/images/speechbubble/speechBubble_close_over.gif\';" onmouseout="this.src=\'images/speechbubble/speechBubble_close.gif\';"></a></div>' +
            '                  <div id="' + this.id + 'Header" style="font-family: Segoe UI; lucida sans; sans serif; font-size: 16px; font-weight: 100; color: #0033aa; left: 50px;' +
            '                    position: absolute; top: 6px">' +
            '                    Data gemt!</div>' +
            '                  <div id="' + this.id + 'Message" style="font-size: 11px; font-weight: normal; color: #000; text-align: left; left: 50px; width: 180px; position: absolute;' +
            '                    top: 28px">' +
            '                    Default Text Container!</div>' +
            '                </div>';
    theBody.appendChild(sbHtml);
}

UmbracoSpeechBubble.prototype.ShowMessage = function(icon, header, message) {
    var speechBubble = document.getElementById(this.id);

    document.getElementById(this.id + "Header").innerHTML = header;
    document.getElementById(this.id + "Message").innerHTML = message;
    document.getElementById(this.id + "IconSrc").src = '/umbraco/images/speechBubble/' + icon + '.png';
    
    speechBubble.style.right = "20px";
    speechBubble.style.bottom = "20px";
    speechBubble.style.visibility = 'visible';
    this.Show(0);
}

UmbracoSpeechBubble.prototype.Show = function(opacity) {
    document.getElementById(this.id).style.filter = 'Alpha(Opacity=' + opacity + ')';

    opacity = parseInt(opacity) + 10;
    
    var _self = this;
    if (opacity < 101) {
        setTimeout(function() {_self.Show(opacity+10);}, 50);
   }  else {
        setTimeout(function() {_self.Hide(100);}, 5000);
    }
}

UmbracoSpeechBubble.prototype.Hide = function(opacity) {
    document.getElementById(this.id).style.filter = 'Alpha(Opacity=' + opacity + ')';

    var _self = this;
    opacity = parseInt(opacity) - 10;
    if (opacity > 1)
        setTimeout(function() {_self.Hide(opacity-10);}, 50);
    else {
        document.getElementById(this.id).style.visibility = 'hidden';
    }
}

// Initialize
var UmbSpeechBubble = null
function InitUmbracoSpeechBubble() {
    if (UmbSpeechBubble == null)
        UmbSpeechBubble = new UmbracoSpeechBubble("defaultSpeechbubble");
}

//if (typeof(addEvent) !== 'undefined') {
//    addEvent(window, "load", InitUmbracoSpeechBubble);
//}else if (Sys != undefined) {
//    Sys.Application.add_load(InitUmbracoSpeechBubble);
//}