		var doScroll = false;
		
		var el = null;
		var FromLeftMax = 0;
		
		// Added NH 2.1
		var scrollIcons = new Array(0);
		
		function RegisterScrollingMenuButtons(elId, Buttons) {
			var icons = Buttons.split(",");
			
			scrollIcons.push(new scrollingContent(elId, icons));
		}
		
		function scrollingContent(Name, Buttons) {
			this.name = Name;
			this.buttons = new Array(Buttons.length);
			for(var i=0;i<this.buttons.length;i++)
				this.buttons[i] = new buttonDef(Buttons[i]);
		}
		
		function buttonDef(Name) {
			this.name = Name;
			this.down = false;
		}
		
		function markIcon(elId, Button) {
			theButton = GetButton(elId, Button);
			document.getElementById(theButton.name).className = 'editorIconDown';
			theButton.down = true;
		}
		
		function hoverIconOut(elId, Button) {
			if(elId != "" && Button != "") {
				theButton = GetButton(elId, Button);
				if (theButton) {
					if (theButton.down)
						document.getElementById(theButton.name).className = 'editorIconDown';
					else
						document.getElementById(theButton.name).className = 'editorIcon';			
				} else
					document.getElementById(theButton.name).className = 'editorIcon';			
				
			} else
				document.getElementById(Button).className = 'editorIcon';						
				
		}
		
		
		function resetIconState(elId) {
			buttons = GetScrollingMenu(elId);
			for(var x=0;x<buttons.length;x++) {
				buttons[x].down = false;
				if (buttons[x].name != "")
					document.getElementById(buttons[x].name).className = 'editorIcon';
				
			}
		}
		
		function GetButton(elId, Button) {
			buttons = GetScrollingMenu(elId);
			for(var x=0;x<buttons.length;x++) {
				if (buttons[x].name == Button)
					return buttons[x];
			}
		}
		
		function GetScrollingMenu(elId) {
			for(var i=0;i<scrollIcons.length;i++) {
				if (scrollIcons[i].name == elId)
					return scrollIcons[i].buttons;
			}
		}
						
		function scrollR(elId, elHid, InnerWidth) {
			doScroll = true;
	 		el = document.getElementById(elId);
	 		FromLeftMax = (InnerWidth - document.getElementById(elHid).offsetWidth)*-1;
	 		scrollHorisontal(-1);
		}
		
		function scrollL(elId, elHid, InnerWidth) {
			doScroll = true;
			el = document.getElementById(elId);
			var hiddenEl = document.getElementById(elHid);
			if (hiddenEl) {
			    FromLeftMax = (InnerWidth - hiddenEl.offsetWidth) * -1;
			    scrollHorisontal(0);
			}
		}
		
		function scrollStop() {
			doScroll = false;
		}
		
		function scrollHorisontal(direction) {
			var mv  = -4;
			if (direction < 0) mv = 4;
			var slFromLeft = (parseInt(el.style.left)+mv);
						
			if ((slFromLeft <= 0 && slFromLeft > FromLeftMax) && doScroll) {
				el.style.left = slFromLeft +"px";
				window.setTimeout("scrollHorisontal(" + direction + ");", 4);
			}
		}