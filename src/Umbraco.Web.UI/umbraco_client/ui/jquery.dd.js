// MSDropDown - uncompressed.jquery.dd
// author: Marghoob Suleman - Search me on google
// Date: 12th Aug, 2009
// Version: 2.38.4 
// Revision: 38
// web: www.giftlelo.com | www.marghoobsuleman.com
/*
// msDropDown is free jQuery Plugin: you can redistribute it and/or modify
// it under the terms of the either the MIT License or the Gnu General Public License (GPL) Version 2
*/
; (function ($) {

    var msOldDiv = "";
    var dd = function (element, options) {
        var sElement = element;
        var $this = this; //parent this
        var options = $.extend({
            height: 120,
            visibleRows: 7,
            rowHeight: 23,
            showIcon: true,
            zIndex: 9999,
            mainCSS: 'dd',
            useSprite: false,
            animStyle: 'slideDown',
            onInit: '',
            jsonTitle: true,
            style: ''
        }, options);
        this.ddProp = new Object(); //storing propeties;
        var oldSelectedValue = "";
        var actionSettings = {};
        actionSettings.insideWindow = true;
        actionSettings.keyboardAction = false;
        actionSettings.currentKey = null;
        var ddList = false;
        var config = { postElementHolder: '_msddHolder', postID: '_msdd', postTitleID: '_title', postTitleTextID: '_titletext', postChildID: '_child', postAID: '_msa', postOPTAID: '_msopta', postInputID: '_msinput', postArrowID: '_arrow', postInputhidden: '_inp' };
        var styles = { dd: options.mainCSS, ddTitle: 'ddTitle', arrow: 'arrow', ddChild: 'ddChild', ddTitleText: 'ddTitleText', disabled: .30, ddOutOfVision: 'ddOutOfVision', borderTop: 'borderTop', noBorderTop: 'noBorderTop', selected: 'selected' };
        var attributes = { actions: "focus,blur,change,click,dblclick,mousedown,mouseup,mouseover,mousemove,mouseout,keypress,keydown,keyup", prop: "size,multiple,disabled,tabindex" };
        this.onActions = new Object();
        var elementid = $(sElement).prop("id");
        if (typeof (elementid) == "undefined" || elementid.length <= 0) {
            //assign and id;
            elementid = "msdrpdd" + $.msDropDown.counter++; //I guess it makes unique for the page.
            $(sElement).attr("id", elementid);
        };
        var inlineCSS = $(sElement).prop("style");
        options.style += (inlineCSS == undefined) ? "" : inlineCSS;
        var allOptions = $(sElement).children();
        ddList = ($(sElement).prop("size") > 1 || $(sElement).prop("multiple") == true) ? true : false;
        if (ddList) { options.visibleRows = $(sElement).prop("size"); };
        var a_array = {}; //stores id, html & value etc
        var currentP = 0;
        var isFilter = false;
        var oldHeight;
        var isClosing = false;
        var cacheElement = {};
        var inputText = "";
        var selectedItem;

        var getElement = function (ele) {
            if (typeof (cacheElement[ele]) == "undefined") {
                cacheElement[ele] = document.getElementById(ele);
            }
            return cacheElement[ele];
        };
        var getPostID = function (id) {
            return elementid + config[id];
        };
        var getOptionsProperties = function (option) {
            var currentOption = option;
            var styles = $(currentOption).prop("style");
            return (typeof styles == "undefined") ? "" : styles.cssText;
        };
        var matchIndex = function (index) {
            if (typeof this.selectedItem === 'undefined')
                this.selectedItem = $("#" + elementid + " option:selected");
            if (this.selectedItem.length > 1) {
                for (var i = 0; i < this.selectedItem.length; i++) {
                    if (index == this.selectedItem[i].index) {
                        return true;
                    };
                };
            } else if (this.selectedItem.length == 1) {
                if (this.selectedItem[0].index == index) {
                    return true;
                };
            };
            return false;
        };
        var createA = function (currentOptOption, current, currentopt, tp) {
            var aTag = "";
            //var aidfix = getPostID("postAID");
            var aidoptfix = (tp == "opt") ? getPostID("postOPTAID") : getPostID("postAID");
            var aid = (tp == "opt") ? aidoptfix + "_" + (current) + "_" + (currentopt) : aidoptfix + "_" + (current);
            var arrow = "";
            var t = "";
            var clsName = "";
            var pH = ""; //addition html

            clsName = ' ' + options.useSprite + ' ' + currentOptOption.className;
            arrow = $(currentOptOption).prop("title");
            var reg = new RegExp(/^\{.*\}$/);
            var isJson = reg.test(arrow);
            if (options.jsonTitle == true && isJson == true) {
                if (arrow.length != 0) {
                    var obj = eval("[" + arrow + "]");
                    img = (typeof obj[0].image == "undefined") ? "" : obj[0].image;
                    t = (typeof obj[0].title == "undefined") ? "" : obj[0].title;
                    pH = (typeof obj[0].postHTML == "undefined") ? "" : obj[0].postHTML;
                    arrow = (img.length == 0) ? "" : '<img src="' + img + '" align="absmiddle" /> ';
                };
            } else {
                arrow = (arrow.length == 0) ? "" : '<img src="' + arrow + '" align="absmiddle" /> ';
            };

            var sText = $(currentOptOption).text();
            var sValue = $(currentOptOption).val();
            var sEnabledClass = ($(currentOptOption).prop("disabled") == true) ? "disabled" : "enabled";
            a_array[aid] = { html: arrow + sText, value: sValue, text: sText, index: currentOptOption.index, id: aid, title: t };
            var innerStyle = getOptionsProperties(currentOptOption);
            if (matchIndex(currentOptOption.index) == true) {
                aTag += '<a href="javascript:void(0);" class="' + styles.selected + ' ' + sEnabledClass + clsName + '"';
            } else {
                aTag += '<a  href="javascript:void(0);" class="' + sEnabledClass + clsName + '"';
            };
            if (innerStyle !== false && innerStyle !== undefined && innerStyle.length != 0) {
                aTag += " style='" + innerStyle + "'";
            };
            if (t !== "") {
                aTag += " title='" + t + "'";
            };
            aTag += ' id="' + aid + '">';
            aTag += arrow + '<span class="' + styles.ddTitleText + '">' + sText + '</span>';
            if (pH !== "") {
                aTag += pH;
            };
            aTag += '</a>';
            return aTag;
        };
        var in_array = function (t) {
            var sText = t.toLowerCase();
            if (sText.length == 0) return -1;
            var a = "";
            for (var i in a_array) {
                var a_text = a_array[i].text.toLowerCase();
                if (a_text.substr(0, sText.length) == sText) {
                    a += "#" + a_array[i].id + ", ";
                };
            };
            return (a == "") ? -1 : a;
        };
        var createATags = function () {
            var childnodes = allOptions;
            if (childnodes.length == 0) return "";
            var aTag = "";
            var aidfix = getPostID("postAID");
            var aidoptfix = getPostID("postOPTAID");
            childnodes.each(function (current) {
                var currentOption = childnodes[current];
                //OPTGROUP
                if (currentOption.nodeName.toString().toLowerCase() == "optgroup") {
                    aTag += "<div class='opta'>";
                    aTag += "<span style='font-weight:bold;font-style:italic;clear:both;'>" + $(currentOption).prop("label") + "</span>";
                    var optChild = $(currentOption).children();
                    optChild.each(function (currentopt) {
                        var currentOptOption = optChild[currentopt];
                        aTag += createA(currentOptOption, current, currentopt, "opt");
                    });
                    aTag += "</div>";

                } else {
                    aTag += createA(currentOption, current, "", "");
                };
            });
            return aTag;
        };
        var createChildDiv = function () {
            var id = getPostID("postID");
            var childid = getPostID("postChildID");
            var sStyle = options.style;
            sDiv = "";
            sDiv += '<div id="' + childid + '" class="' + styles.ddChild + '"';
            if (!ddList) {
                sDiv += (sStyle != "") ? ' style="' + sStyle + '"' : '';
            } else {
                sDiv += (sStyle != "") ? ' style="border-top:1px solid #c3c3c3;display:block;position:relative;' + sStyle + '"' : '';
            };
            sDiv += '>';
            return sDiv;
        };

        var createTitleDiv = function () {
            var titleid = getPostID("postTitleID");
            var arrowid = getPostID("postArrowID");
            var titletextid = getPostID("postTitleTextID");
            var inputhidden = getPostID("postInputhidden");
            var sText = "";
            var arrow = "";
            if (getElement(elementid).options.length > 0) {
                sText = $("#" + elementid + " option:selected").text();
                arrow = $("#" + elementid + " option:selected").prop("title");
            };
            var img = "";
            var t = "";
            var reg = new RegExp(/^\{.*\}$/);
            var isJson = reg.test(arrow);
            if (options.jsonTitle == true && isJson == true) {
                if (arrow.length != 0) {
                    var obj = eval("[" + arrow + "]");
                    img = (typeof obj[0].image == "undefined") ? "" : obj[0].image;
                    t = (typeof obj[0].title == "undefined") ? "" : obj[0].title;
                    arrow = (img.length == 0 || options.showIcon == false || options.useSprite != false) ? "" : '<img src="' + img + '" align="absmiddle" /> ';
                };
            } else {
                arrow = (arrow.length == 0 || arrow == undefined || options.showIcon == false || options.useSprite != false) ? "" : '<img src="' + arrow + '" align="absmiddle" /> ';
            };
            var sDiv = '<div id="' + titleid + '" class="' + styles.ddTitle + '"';
            sDiv += '>';
            sDiv += '<span id="' + arrowid + '" class="' + styles.arrow + '"></span><span class="' + styles.ddTitleText + '" id="' + titletextid + '">' + arrow + '<span class="' + styles.ddTitleText + '">' + sText + '</span></span></div>';
            return sDiv;
        };
        var applyEventsOnA = function () {
            var childid = getPostID("postChildID");
            $("#" + childid + " a.enabled").unbind("click"); //remove old one
            $("#" + childid + " a.enabled").bind("click", function (event) {
                event.preventDefault();
                manageSelection(this);
                setValue();
                if (!ddList) {
                    $("#" + childid).unbind("mouseover");
                    setInsideWindow(false);
                    var sText = (options.showIcon == false) ? $(this).text() : $(this).html();
                    setTitleText(sText);
                    //$this.data("dd").close();
                    $this.close();
                };
                //actionSettings.oldIndex = a_array[$($this).prop("id")].index;
            });
        };
        var createDropDown = function () {
            var changeInsertionPoint = false;
            var id = getPostID("postID");
            var titleid = getPostID("postTitleID");
            var titletextid = getPostID("postTitleTextID");
            var childid = getPostID("postChildID");
            var arrowid = getPostID("postArrowID");
            var iWidth = $("#" + elementid).outerWidth();
            var sStyle = options.style;
            if ($("#" + id).length > 0) {
                $("#" + id).remove();
                changeInsertionPoint = true;
            };
            var sDiv = '<div id="' + id + '" class="' + styles.dd + '"';
            sDiv += (sStyle != "") ? ' style="' + sStyle + '"' : '';
            sDiv += '>';
            //create title bar
            sDiv += createTitleDiv();
            //create child
            sDiv += createChildDiv();
            sDiv += createATags();
            sDiv += "</div>";
            sDiv += "</div>";
            if (changeInsertionPoint == true) {
                var sid = getPostID("postElementHolder");
                $("#" + sid).after(sDiv);
            } else {
                $("#" + elementid).after(sDiv);
            };
            if (ddList) {
                var titleid = getPostID("postTitleID");
                $("#" + titleid).hide();
            };

            $("#" + id).css("width", iWidth + "px");
            $("#" + childid).css("width", (iWidth - 2) + "px");
            if (allOptions.length > options.visibleRows) {
                var margin = parseInt($("#" + childid + " a:first").css("padding-bottom")) + parseInt($("#" + childid + " a:first").css("padding-top"));
                var iHeight = ((options.rowHeight) * options.visibleRows) - margin;
                $("#" + childid).css("height", iHeight + "px");
            } else if (ddList) {
                var iHeight = $("#" + elementid).height();
                $("#" + childid).css("height", iHeight + "px");
            };
            //set out of vision
            if (changeInsertionPoint == false) {
                setOutOfVision();
                addRefreshMethods(elementid);
            };
            if ($("#" + elementid).prop("disabled") == true) {
                $("#" + id).css("opacity", styles.disabled);
            };
            applyEvents();
            //add events
            //arrow hightlight
            $("#" + titleid).bind("mouseover", function (event) {
                hightlightArrow(1);
            });
            $("#" + titleid).bind("mouseout", function (event) {
                hightlightArrow(0);
            });
            //open close events
            applyEventsOnA();
            $("#" + childid + " a.disabled").css("opacity", styles.disabled);
            //alert("ddList "+ddList)
            if (ddList) {
                $("#" + childid).bind("mouseover", function (event) {
                    if (!actionSettings.keyboardAction) {
                        actionSettings.keyboardAction = true;
                        $(document).bind("keydown", function (event) {
                            var keyCode = event.keyCode;
                            actionSettings.currentKey = keyCode;
                            if (keyCode == 39 || keyCode == 40) {
                                //move to next
                                event.preventDefault(); event.stopPropagation();
                                next();
                                setValue();
                            };
                            if (keyCode == 37 || keyCode == 38) {
                                event.preventDefault(); event.stopPropagation();
                                //move to previous
                                previous();
                                setValue();
                            };
                        });

                    }
                });
            };
            $("#" + childid).bind("mouseout", function (event) { setInsideWindow(false); $(document).unbind("keydown", d_onkeydown); actionSettings.keyboardAction = false; actionSettings.currentKey = null; });
            $("#" + titleid).bind("click", function (event) {
                setInsideWindow(false);
                if ($("#" + childid + ":visible").length == 1) {
                    $("#" + childid).unbind("mouseover");
                } else {
                    $("#" + childid).bind("mouseover", function (event) { setInsideWindow(true); });
                    //alert("open "+elementid + $this);
                    //$this.data("dd").openMe();
                    $this.open();
                };
            });
            $("#" + titleid).bind("mouseout", function (evt) {
                setInsideWindow(false);
            });
            if (options.showIcon && options.useSprite != false) {
                setTitleImageSprite();
            };
        };
        var getByIndex = function (index) {
            for (var i in a_array) {
                if (a_array[i].index == index) {
                    return a_array[i];
                };
            };
            return -1;
        };
        var manageSelection = function (obj) {
            var childid = getPostID("postChildID");
            if ($("#" + childid + " a." + styles.selected).length == 1) { //check if there is any selected
                oldSelectedValue = $("#" + childid + " a." + styles.selected).text(); //i should have value here. but sometime value is missing
                //alert("oldSelectedValue "+oldSelectedValue);
            };
            if (!ddList) {
                $("#" + childid + " a." + styles.selected).removeClass(styles.selected);
            };
            var selectedA = $("#" + childid + " a." + styles.selected).prop("id");
            if (selectedA != undefined) {
                var oldIndex = (actionSettings.oldIndex == undefined || actionSettings.oldIndex == null) ? a_array[selectedA].index : actionSettings.oldIndex;
            };
            if (obj && !ddList) {
                $(obj).addClass(styles.selected);
            };
            if (ddList) {
                var keyCode = actionSettings.currentKey;
                if ($("#" + elementid).prop("multiple") == true) {
                    if (keyCode == 17) {
                        //control
                        actionSettings.oldIndex = a_array[$(obj).prop("id")].index;
                        $(obj).toggleClass(styles.selected);
                        //multiple
                    } else if (keyCode == 16) {
                        $("#" + childid + " a." + styles.selected).removeClass(styles.selected);
                        $(obj).addClass(styles.selected);
                        //shift
                        var currentSelected = $(obj).prop("id");
                        var currentIndex = a_array[currentSelected].index;
                        for (var i = Math.min(oldIndex, currentIndex) ; i <= Math.max(oldIndex, currentIndex) ; i++) {
                            $("#" + getByIndex(i).id).addClass(styles.selected);
                        };
                    } else {
                        $("#" + childid + " a." + styles.selected).removeClass(styles.selected);
                        $(obj).addClass(styles.selected);
                        actionSettings.oldIndex = a_array[$(obj).prop("id")].index;
                    };
                } else {
                    $("#" + childid + " a." + styles.selected).removeClass(styles.selected);
                    $(obj).addClass(styles.selected);
                    actionSettings.oldIndex = a_array[$(obj).prop("id")].index;
                };
                //isSingle
            };
        };
        var addRefreshMethods = function (id) {
            //deprecated
            var objid = id;
            getElement(objid).refresh = function (e) {
                $("#" + objid).msDropDown(options);
            };
        };
        var setInsideWindow = function (val) {
            actionSettings.insideWindow = val;
        };
        var getInsideWindow = function () {
            return actionSettings.insideWindow;
            //will work on this
            /*
            var childid = getPostID("postChildID");
            return ($("#"+childid + ":visible").length == 0) ? false : true;
            */
        };
        var applyEvents = function () {
            var mainid = getPostID("postID");
            var actions_array = attributes.actions.split(",");
            for (var iCount = 0; iCount < actions_array.length; iCount++) {
                var action = actions_array[iCount];
                //var actionFound = $("#"+elementid).prop(action);
                var actionFound = has_handler(action);
                if (actionFound == true) {
                    switch (action) {
                        case "focus":
                            $("#" + mainid).bind("mouseenter", function (event) {
                                getElement(elementid).focus();
                                //$("#"+elementid).focus();
                            });
                            break;
                        case "click":
                            $("#" + mainid).bind("click", function (event) {
                                //getElement(elementid).onclick();
                                $("#" + elementid).trigger("click");
                            });
                            break;
                        case "dblclick":
                            $("#" + mainid).bind("dblclick", function (event) {
                                //getElement(elementid).ondblclick();
                                $("#" + elementid).trigger("dblclick");
                            });
                            break;
                        case "mousedown":
                            $("#" + mainid).bind("mousedown", function (event) {
                                //getElement(elementid).onmousedown();
                                $("#" + elementid).trigger("mousedown");
                            });
                            break;
                        case "mouseup":
                            //has in close mthod
                            $("#" + mainid).bind("mouseup", function (event) {
                                //getElement(elementid).onmouseup();
                                $("#" + elementid).trigger("mouseup");
                                //setValue();
                            });
                            break;
                        case "mouseover":
                            $("#" + mainid).bind("mouseover", function (event) {
                                //getElement(elementid).onmouseover();													   
                                $("#" + elementid).trigger("mouseover");
                            });
                            break;
                        case "mousemove":
                            $("#" + mainid).bind("mousemove", function (event) {
                                //getElement(elementid).onmousemove();
                                $("#" + elementid).trigger("mousemove");
                            });
                            break;
                        case "mouseout":
                            $("#" + mainid).bind("mouseout", function (event) {
                                //getElement(elementid).onmouseout();
                                $("#" + elementid).trigger("mouseout");
                            });
                            break;
                    };
                };
            };

        };
        var setOutOfVision = function () {
            var sId = getPostID("postElementHolder");
            $("#" + elementid).after("<div class='" + styles.ddOutOfVision + "' style='height:0px;overflow:hidden;position:absolute;' id='" + sId + "'></div>");
            $("#" + elementid).appendTo($("#" + sId));
        };
        var setTitleText = function (sText) {
            var titletextid = getPostID("postTitleTextID");
            $("#" + titletextid).html(sText);
        };
        var navigateA = function (w) {
            var where = w;
            var childid = getPostID("postChildID");
            var visibleA = $("#" + childid + " a:visible");
            var totalA = visibleA.length;
            var currentP = $("#" + childid + " a:visible").index($("#" + childid + " a.selected:visible"));
            var nextA;
            switch (where) {
                case "next":
                    if (currentP < totalA - 1) {
                        currentP++;
                        nextA = visibleA[currentP];
                    };
                    break;
                case "previous":
                    if (currentP < totalA && currentP > 0) {
                        currentP--;
                        nextA = visibleA[currentP];
                    };
                    break;
            };
            if (typeof (nextA) == "undefined") {
                return false;
            };
            $("#" + childid + " a." + styles.selected).removeClass(styles.selected);
            $(nextA).addClass(styles.selected);
            var selectedA = nextA.id;
            if (!ddList) {
                var sText = (options.showIcon == false) ? a_array[selectedA].text : $("#" + selectedA).html();
                setTitleText(sText);
                setTitleImageSprite(a_array[selectedA].index);
            };
            if (where == "next") {
                if (parseInt(($("#" + selectedA).position().top + $("#" + selectedA).height())) >= parseInt($("#" + childid).height())) {
                    $("#" + childid).scrollTop(($("#" + childid).scrollTop()) + $("#" + selectedA).height() + $("#" + selectedA).height());
                };
            } else {
                if (parseInt(($("#" + selectedA).position().top + $("#" + selectedA).height())) <= 0) {
                    $("#" + childid).scrollTop(($("#" + childid).scrollTop() - $("#" + childid).height()) - $("#" + selectedA).height());
                };
            };
        };
        var next = function () {
            navigateA("next");
        };
        var previous = function () {
            navigateA("previous");
        };
        var setTitleImageSprite = function (i) {
            if (options.useSprite != false) {
                var titletextid = getPostID("postTitleTextID");
                var index = (typeof (i) == "undefined") ? getElement(elementid).selectedIndex : i;
                var sClassName = getElement(elementid).options[index].className;
                if (sClassName.length > 0) {
                    var childid = getPostID("postChildID");
                    var id = $("#" + childid + " a." + sClassName).prop("id");
                    var backgroundImg = $("#" + id).css("background-image");
                    var backgroundPosition = $("#" + id).css("background-position");
                    if (backgroundPosition == undefined) {
                        backgroundPosition = $("#" + id).css("background-position-x") + " " + $("#" + id).css("background-position-y");
                    };
                    var paddingLeft = $("#" + id).css("padding-left");
                    if (backgroundImg != undefined) {
                        $("#" + titletextid).find("." + styles.ddTitleText).attr('style', "background:" + backgroundImg);
                    };
                    if (backgroundPosition != undefined) {
                        $("#" + titletextid).find("." + styles.ddTitleText).css('background-position', backgroundPosition);
                    };
                    if (paddingLeft != undefined) {
                        $("#" + titletextid).find("." + styles.ddTitleText).css('padding-left', paddingLeft);
                    };
                    $("#" + titletextid).find("." + styles.ddTitleText).css('background-repeat', 'no-repeat');
                    $("#" + titletextid).find("." + styles.ddTitleText).css('padding-bottom', '2px');
                };
            };
        };
        var setValue = function () {
            //alert("setValue "+elementid);
            var childid = getPostID("postChildID");
            var allSelected = $("#" + childid + " a." + styles.selected);
            if (allSelected.length == 1) {
                var sText = $("#" + childid + " a." + styles.selected).text();
                var selectedA = $("#" + childid + " a." + styles.selected).prop("id");
                if (selectedA != undefined) {
                    var sValue = a_array[selectedA].value;
                    getElement(elementid).selectedIndex = a_array[selectedA].index;
                };
                //set image on title if using sprite

                if (options.showIcon && options.useSprite != false)
                    setTitleImageSprite();
            } else if (allSelected.length > 1) {
                //var alls = $("#"+elementid +" > option:selected").removeprop("selected");
                for (var i = 0; i < allSelected.length; i++) {
                    var selectedA = $(allSelected[i]).prop("id");
                    var index = a_array[selectedA].index;
                    getElement(elementid).options[index].selected = "selected";
                };
            };
            //alert(getElement(elementid).selectedIndex);
            var sIndex = getElement(elementid).selectedIndex;
            $this.ddProp["selectedIndex"] = sIndex;
            //alert("selectedIndex "+ $this.ddProp["selectedIndex"] + " sIndex "+sIndex);
        };
        var has_handler = function (name) {
            // True if a handler has been added in the html.
            if ($("#" + elementid).prop("on" + name) != undefined) {
                return true;
            };
            // True if a handler has been added using jQuery.
            var evs = $("#" + elementid).data("events");
            if (evs && evs[name]) {
                return true;
            };
            return false;
        };
        var blur_m = function (evt) {
            $("#" + elementid).focus();
            $("#" + elementid)[0].blur();
            setValue();
            $(document).unbind("mouseup", d_onmouseup);
            $(document).unbind("mouseup", blur_m);
        };
        var checkMethodAndApply = function () {
            //console.log("calling checkMethodAndApply");
            var childid = getPostID("postChildID");
            if (has_handler('change') == true) {
                //alert(1);
                var currentSelected = a_array[$("#" + childid + " a.selected").prop("id")];
                if (currentSelected != undefined) {
                    var currentSelectedValue = currentSelected.text;
                    if ($.trim(oldSelectedValue) !== $.trim(currentSelectedValue) && oldSelectedValue !== "") {
                        $("#" + elementid).trigger("change");
                    };
                }
            };
            if (has_handler('mouseup') == true) {
                $("#" + elementid).trigger("mouseup");
            };
            if (has_handler('blur') == true) {
                $(document).bind("mouseup", blur_m);
            };
            return false;
        };
        var hightlightArrow = function (ison) {
            var arrowid = getPostID("postArrowID");
            if (ison == 1)
                $("#" + arrowid).css({ backgroundPosition: '0 100%' });
            else
                $("#" + arrowid).css({ backgroundPosition: '0 0' });
        };
        var setOriginalProperties = function () {
            //properties = {};		
            for (var i in getElement(elementid)) {
                if (typeof (getElement(elementid)[i]) !== 'function' && typeof (getElement(elementid)[i]) !== "undefined" && typeof (getElement(elementid)[i]) !== "null") {
                    $this.set(i, getElement(elementid)[i], true); //true = setting local properties
                };
            };
        };
        var setValueByIndex = function (prop, val) {
            if (getByIndex(val) != -1) {
                getElement(elementid)[prop] = val;
                var childid = getPostID("postChildID");
                $("#" + childid + " a." + styles.selected).removeClass(styles.selected);
                $("#" + getByIndex(val).id).addClass(styles.selected);
                var sText = getByIndex(getElement(elementid).selectedIndex).html;
                setTitleText(sText);
            };
        };
        var addRemoveFromIndex = function (i, action) {
            if (action == 'd') {
                for (var key in a_array) {
                    if (a_array[key].index == i) {
                        delete a_array[key];
                        break;
                    };
                };
            };
            //update index
            var count = 0;
            for (var key in a_array) {
                a_array[key].index = count;
                count++;
            };
        };
        var shouldOpenOpposite = function () {
            var childid = getPostID("postChildID");
            var main = getPostID("postID");
            var pos = $("#" + main).offset();
            var mH = $("#" + main).height();
            var wH = $(window).height();
            var st = $(window).scrollTop();
            var cH = $("#" + childid).height();
            var css = { zIndex: options.zIndex, top: (mH) + "px", display: "none" };
            var ani = options.animStyle;
            var opp = false;
            var borderTop = styles.noBorderTop;
            $("#" + childid).removeClass(styles.noBorderTop);
            $("#" + childid).removeClass(styles.borderTop);
            if ((wH + st) < Math.floor(cH + mH + pos.top)) {
                var tp = cH;
                css = { zIndex: options.zIndex, top: "-" + tp + "px", display: "none" };
                ani = "show";
                opp = true;
                borderTop = styles.borderTop;
            };
            return { opp: opp, ani: ani, css: css, border: borderTop };
        };
        var fireOpenEvent = function () {
            if ($this.onActions["onOpen"] != null) {
                eval($this.onActions["onOpen"])($this);
            };
        };
        var fireCloseEvent = function () {
            checkMethodAndApply();
            if ($this.onActions["onClose"] != null) {
                eval($this.onActions["onClose"])($this);
            };
        };
        var d_onkeydown = function (event) {
            var childid = getPostID("postChildID");
            var keyCode = event.keyCode;
            //alert("keyCode "+keyCode);
            if (keyCode == 8) {
                event.preventDefault(); event.stopPropagation();
                //remove char
                inputText = (inputText.length == 0) ? "" : inputText.substr(0, inputText.length - 1);
            };
            switch (keyCode) {
                case 39:
                case 40:
                    //move to next
                    event.preventDefault(); event.stopPropagation();
                    next();
                    break;
                case 37:
                case 38:
                    //move to previous
                    event.preventDefault(); event.stopPropagation();
                    previous();
                    break;
                case 27:
                case 13:
                    $this.close();
                    setValue();
                    break;
                default:
                    if (keyCode > 46) {
                        inputText += String.fromCharCode(keyCode);
                    };
                    var ind = in_array(inputText);
                    if (ind != -1) {
                        $("#" + childid).css({ height: 'auto' });
                        $("#" + childid + " a").hide();
                        $(ind).show();
                        var wf = shouldOpenOpposite();
                        $("#" + childid).css(wf.css);
                        $("#" + childid).css({ display: 'block' });
                    } else {
                        $("#" + childid + " a").show();
                        $("#" + childid).css({ height: oldHeight + 'px' });
                    };
                    break;
            };
            if (has_handler("keydown") == true) {
                getElement(elementid).onkeydown();
            };
            return false;
        };
        var d_onmouseup = function (event) {
            if (getInsideWindow() == false) {
                //alert("evt.target: "+event.target);
                //$this.data("dd").close();
                $this.close();
            };
            return false;
        };
        var d_onkeyup = function (event) {
            if ($("#" + elementid).prop("onkeyup") != undefined) {
                //$("#"+elementid).keyup();
                getElement(elementid).onkeyup();
            };
            return false;
        };
        /************* public methods *********************/
        this.open = function () {
            if (($this.get("disabled", true) == true) || ($this.get("options", true).length == 0)) return;
            var childid = getPostID("postChildID");
            if (msOldDiv != "" && childid != msOldDiv) {
                $("#" + msOldDiv).slideUp("fast");
                $("#" + msOldDiv).css({ zIndex: '0' });
            };
            if ($("#" + childid).css("display") == "none") {
                var oldSelected = a_array[$("#" + childid + " a.selected").prop("id")];
                if (oldSelected != undefined)
                    oldSelectedValue = oldSelected.text;

                //keyboard action
                inputText = "";
                oldHeight = $("#" + childid).height();
                $("#" + childid + " a").show();
                $(document).bind("keydown", d_onkeydown);
                $(document).bind("keyup", d_onkeyup);
                //end keyboard action

                //close onmouseup
                $(document).bind("mouseup", d_onmouseup);

                //check open
                var wf = shouldOpenOpposite();
                $("#" + childid).css(wf.css);
                if (wf.opp == true) {
                    $("#" + childid).css({ display: 'block' });
                    $("#" + childid).addClass(wf.border);
                    fireOpenEvent();
                } else {
                    $("#" + childid)[wf.ani]("fast", function () {
                        $("#" + childid).addClass(wf.border);
                        fireOpenEvent();
                    });
                };
                if (childid != msOldDiv) {
                    msOldDiv = childid;
                };
            };
        };
        this.close = function () {
            var childid = getPostID("postChildID");
            if (!$("#" + childid).is(":visible") || isClosing) return;
            isClosing = true;
            //console.log("calling close " + $("#"+childid).css("display"));
            if ($("#" + childid).css("display") == "none") { return false; };
            var top = $("#" + getPostID("postTitleID")).position().top;
            var wf = shouldOpenOpposite();
            //var oldHeight = $("#"+childid).height();
            isFilter = false;
            if (wf.opp == true) {
                $("#" + childid).animate({
                    height: 0,
                    top: top
                }, function () {
                    $("#" + childid).css({ height: oldHeight + 'px', display: 'none' });
                    fireCloseEvent();
                    isClosing = false;
                });
            }
            else {
                $("#" + childid).slideUp("fast", function (event) {
                    fireCloseEvent();
                    $("#" + childid).css({ zIndex: '0' });
                    $("#" + childid).css({ height: oldHeight + 'px' });
                    isClosing = false;
                });
            };
            setTitleImageSprite();
            $(document).unbind("keydown", d_onkeydown);
            $(document).unbind("keyup", d_onkeyup);
            $(document).unbind("mouseup", d_onmouseup);
        };
        this.selectedIndex = function (i) {
            if (typeof (i) == "undefined") {
                return $this.get("selectedIndex");
            } else {
                $this.set("selectedIndex", i);
            };
        };
        this.debug = function (is) {
            if (typeof (is) == "undefined" || is == true) {
                $("." + styles.ddOutOfVision).removeAttr("style");
            } else {
                $("." + styles.ddOutOfVision).attr("style", "height:0px;overflow:hidden;position:absolute");
            };
        };
        //update properties
        this.set = function (prop, val, isLocal) {
            //alert("- set " + prop + " : "+val);
            if (typeof prop == "undefined" || typeof val == "undefined") return false;
            $this.ddProp[prop] = val;
            if (isLocal != true) {
                switch (prop) {
                    case "selectedIndex":
                        setValueByIndex(prop, val);
                        break;
                    case "disabled":
                        $this.disabled(val, true);
                        break;
                    case "multiple":
                        getElement(elementid)[prop] = val;
                        ddList = ($(sElement).prop("size") > 0 || $(sElement).prop("multiple") == true) ? true : false;
                        if (ddList) {
                            //do something
                            var iHeight = $("#" + elementid).height();
                            var childid = getPostID("postChildID");
                            $("#" + childid).css("height", iHeight + "px");
                            //hide titlebar
                            var titleid = getPostID("postTitleID");
                            $("#" + titleid).hide();
                            var childid = getPostID("postChildID");
                            $("#" + childid).css({ display: 'block', position: 'relative' });
                            applyEventsOnA();
                        };
                        break;
                    case "size":
                        getElement(elementid)[prop] = val;
                        if (val == 0) {
                            getElement(elementid).multiple = false;
                        };
                        ddList = ($(sElement).prop("size") > 0 || $(sElement).prop("multiple") == true) ? true : false;
                        if (val == 0) {
                            //show titlebar
                            var titleid = getPostID("postTitleID");
                            $("#" + titleid).show();
                            var childid = getPostID("postChildID");
                            $("#" + childid).css({ display: 'none', position: 'absolute' });
                            var sText = "";
                            if (getElement(elementid).selectedIndex >= 0) {
                                var aObj = getByIndex(getElement(elementid).selectedIndex);
                                sText = aObj.html;
                                manageSelection($("#" + aObj.id));
                            };
                            setTitleText(sText);
                        } else {
                            //hide titlebar
                            var titleid = getPostID("postTitleID");
                            $("#" + titleid).hide();
                            var childid = getPostID("postChildID");
                            $("#" + childid).css({ display: 'block', position: 'relative' });
                        };
                        break;
                    default:
                        try {
                            //check if this is not a readonly properties
                            getElement(elementid)[prop] = val;
                        } catch (e) {
                            //silent
                        };
                        break;
                };
            };
            //alert("get " + prop + " : "+$this.ddProp[prop]);
            //$this.set("selectedIndex", 0);
        };
        this.get = function (prop, forceRefresh) {
            if (prop == undefined && forceRefresh == undefined) {
                //alert("c1 : " +$this.ddProp);
                return $this.ddProp;
            };
            if (prop != undefined && forceRefresh == undefined) {
                //alert("c2 : " +$this.ddProp[prop]);
                return ($this.ddProp[prop] != undefined) ? $this.ddProp[prop] : null;
            };
            if (prop != undefined && forceRefresh != undefined) {
                //alert("c3 : " +getElement(elementid)[prop]);
                return getElement(elementid)[prop];
            };
        };
        this.visible = function (val) {
            var id = getPostID("postID");
            if (val == true) {
                $("#" + id).show();
            } else if (val == false) {
                $("#" + id).hide();
            } else {
                return $("#" + id).css("display");
            };
        };
        this.add = function (opt, index) {
            var objOpt = opt;
            var sText = objOpt.text;
            var sValue = (objOpt.value == undefined || objOpt.value == null) ? sText : objOpt.value;
            var img = (objOpt["title"] == undefined || objOpt["title"] == null) ? '' : objOpt["title"];
            var i = (index == undefined || index == null) ? getElement(elementid).options.length : index;
            getElement(elementid).options[i] = new Option(sText, sValue);
            if (img != '') getElement(elementid).options[i]["title"] = img;
            //check if exist
            var ifA = getByIndex(i);
            if (ifA != -1) {
                //replace
                var aTag = createA(getElement(elementid).options[i], i, "", "");
                $("#" + ifA.id).html(aTag);
                //a_array[key]
            } else {
                var aTag = createA(getElement(elementid).options[i], i, "", "");
                //add
                var childid = getPostID("postChildID");
                $("#" + childid).append(aTag);
                applyEventsOnA();
            };
        };
        this.remove = function (i) {
            getElement(elementid).remove(i);
            if ((getByIndex(i)) != -1) { $("#" + getByIndex(i).id).remove(); addRemoveFromIndex(i, 'd'); };
            //alert("a" +a);
            if (getElement(elementid).length == 0) {
                setTitleText("");
            } else {
                var sText = getByIndex(getElement(elementid).selectedIndex).html;
                setTitleText(sText);
            };
            $this.set("selectedIndex", getElement(elementid).selectedIndex);
        };
        this.disabled = function (dis, isLocal) {
            getElement(elementid).disabled = dis;
            //alert(getElement(elementid).disabled);
            var id = getPostID("postID");
            if (dis == true) {
                $("#" + id).css("opacity", styles.disabled);
                $this.close();
            } else if (dis == false) {
                $("#" + id).css("opacity", 1);
            };
            if (isLocal != true) {
                $this.set("disabled", dis);
            };
        };
        //return form element
        this.form = function () {
            return (getElement(elementid).form == undefined) ? null : getElement(elementid).form;
        };
        this.item = function () {
            //index, subindex - use arguments.length
            if (arguments.length == 1) {
                return getElement(elementid).item(arguments[0]);
            } else if (arguments.length == 2) {
                return getElement(elementid).item(arguments[0], arguments[1]);
            } else {
                throw { message: "An index is required!" };
            };
        };
        this.namedItem = function (nm) {
            return getElement(elementid).namedItem(nm);
        };
        this.multiple = function (is) {
            if (typeof (is) == "undefined") {
                return $this.get("multiple");
            } else {
                $this.set("multiple", is);
            };

        };
        this.size = function (sz) {
            if (typeof (sz) == "undefined") {
                return $this.get("size");
            } else {
                $this.set("size", sz);
            };
        };
        this.addMyEvent = function (nm, fn) {
            $this.onActions[nm] = fn;
        };
        this.fireEvent = function (nm) {
            eval($this.onActions[nm])($this);
        };
        this.showRows = function (r) {
            if (typeof r == "undefined" || r == 0) { return false };
            var childid = getPostID("postChildID");
            var fc = $("#" + childid + " a:first").height();
            var dh = (fc == 0) ? options.rowHeight : fc;
            var iHeight = r * dh;
            $("#" + childid).css("height", iHeight + "px");
        };
        //end 
        var updateCommonVars = function () {
            $this.set("version", $.msDropDown.version);
            $this.set("author", $.msDropDown.author);
        };
        var init = function () {
            //create wrapper
            createDropDown();
            //update propties
            //alert("init");
            setOriginalProperties();
            updateCommonVars();
            if (options.onInit != '') {
                eval(options.onInit)($this);
            };
        };
        init();
    };
    //static
    $.msDropDown = {
        version: '2.38.4',
        author: "Marghoob Suleman",
        counter: 20,
        debug: function (v) {
            if (v == true) {
                $(".ddOutOfVision").css({ height: '20px', position: 'relative' });
            } else {
                $(".ddOutOfVision").css({ height: '0px', position: 'absolute' });
            };
        },
        create: function (id, opt) {
            return $(id).msDropDown(opt).data("dd");
        }
    };
    $.fn.extend({
        msDropDown: function (options) {
            return this.each(function () {
                //if ($(this).data('dd')) return; // need to comment when using refresh method - will remove in next version
                var mydropdown = new dd(this, options);
                $(this).data('dd', mydropdown);
            });
        }
    });
    //fixed for prop
    if (typeof ($.fn.prop) == 'undefined') {
        $.fn.prop = function (w, v) {
            if (typeof v == "undefined") {
                return $(this).attr(w);
            };
            try {
                $(this).attr(w, v);
            } catch (e) {
                //some properties are read only.
            };
        };
    };

})(jQuery);