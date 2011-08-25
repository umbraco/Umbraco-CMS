// MSDropDown - jquery.dd.js
// author: Marghoob Suleman
// Date: 12th Aug, 2009
// Version: 2.1 {date: 3rd Sep 2009}
// Revision: 25
// web: www.giftlelo.com | www.marghoobsuleman.com
// MSDropDown - jquery.dd.js
// author: Marghoob Suleman
// Date: 12th Aug, 2009
// Version: 2.1 {date: 3rd Sep 2009}
// Revision: 25
// web: www.giftlelo.com | www.marghoobsuleman.com
/*
// msDropDown is free jQuery Plugin: you can redistribute it and/or modify
// it under the terms of the either the MIT License or the Gnu General Public License (GPL) Version 2
*/
; (function($) {
    var oldDiv = "";
    $.fn.dd = function(options) {
        $this = this;
        options = $.extend({
            height: 120,
            visibleRows: 7,
            rowHeight: 23,
            showIcon: true,
            zIndex: 9999,
            style: ''
        }, options);
        var selectedValue = "";
        var actionSettings = {};
        actionSettings.insideWindow = true;
        actionSettings.keyboardAction = false;
        actionSettings.currentKey = null;
        var ddList = false;
        config = { postElementHolder: '_msddHolder', postID: '_msdd', postTitleID: '_title', postTitleTextID: '_titletext', postChildID: '_child', postAID: '_msa', postOPTAID: '_msopta', postInputID: '_msinput', postArrowID: '_arrow', postInputhidden: '_inp' };
        styles = { dd: 'dd', ddTitle: 'ddTitle', arrow: 'arrow', ddChild: 'ddChild', disbaled: .30 };
        attributes = { actions: "onfocus,onblur,onchange,onclick,ondblclick,onmousedown,onmouseup,onmouseover,onmousemove,onmouseout,onkeypress,onkeydown,onkeyup", prop: "size,multiple,disabled,tabindex" };
        var elementid = $(this).attr("id");
        var inlineCSS = $(this).attr("style");
        options.style += (inlineCSS == undefined) ? "" : inlineCSS;
        var allOptions = $(this).children();
        ddList = ($(this).attr("size") > 0 || $(this).attr("multiple") == true) ? true : false;
        if (ddList) { options.visibleRows = $(this).attr("size"); };
        var a_array = {}; //stores id, html & value etc

        var cacheElement = {};
        var getElement = function(ele) {
		    if(typeof(cacheElement[ele])=="undefined") {
			    cacheElement[ele] = document.getElementById(ele);
		    }
		    return cacheElement[ele];
	    };

        //create wrapper
        createDropDown();

        function getPostID(id) {
            return elementid + config[id];
        };
        function getOptionsProperties(option) {
            var currentOption = option;
            var styles = $(currentOption).attr("style");
            return styles;
        };
        function matchIndex(index) {
            var selectedIndex = $("#" + elementid + " option:selected");
            if (selectedIndex.length > 1) {
                for (var i = 0; i < selectedIndex.length; i++) {
                    if (index == selectedIndex[i].index) {
                        return true;
                    };
                };
            } else if (selectedIndex.length == 1) {
                if (selectedIndex[0].index == index) {
                    return true;
                };
            };
            return false;
        }
        function createATags() {
            var childnodes = allOptions;
            var aTag = "";
            var aidfix = getPostID("postAID");
            var aidoptfix = getPostID("postOPTAID");
            childnodes.each(function(current) {
                var currentOption = childnodes[current];
                //OPTGROUP
                if (currentOption.nodeName == "OPTGROUP") {
                    aTag += "<div class='opta'>";
                    aTag += "<span style='font-weight:bold;font-style:italic; clear:both;'>" + $(currentOption).attr("label") + "</span>";
                    var optChild = $(currentOption).children();
                    optChild.each(function(currentopt) {
                        var currentOptOption = optChild[currentopt];
                        var aid = aidoptfix + "_" + (current) + "_" + (currentopt);
                        var arrow = $(currentOptOption).attr("title");
                        arrow = (arrow.length == 0) ? "" : '<img src="' + arrow + '" align="left" /> ';
                        var sText = $(currentOptOption).text();
                        var sValue = $(currentOptOption).val();
                        var sEnabledClass = ($(currentOptOption).attr("disabled") == true) ? "disabled" : "enabled";
                        a_array[aid] = { html: arrow + sText, value: sValue, text: sText, index: currentOptOption.index, id: aid };
                        var innerStyle = getOptionsProperties(currentOptOption);
                        if (matchIndex(currentOptOption.index) == true) {
                            aTag += '<a href="javascript:void(0);" class="selected ' + sEnabledClass + '"';
                        } else {
                            aTag += '<a  href="javascript:void(0);" class="' + sEnabledClass + '"';
                        };
                        if (innerStyle != false)
                            aTag += ' style="' + innerStyle + '"';
                        aTag += ' id="' + aid + '">';
                        aTag += arrow + sText + '</a>';
                    });
                    aTag += "</div>";

                } else {
                    var aid = aidfix + "_" + (current);
                    // custom update by tg, needed after jquery and jquery ui update
                    var arrow ="";
                    arrow = $(currentOption).prop("title");
                    // old line was: 
                    //var arrow = $(currentOption).attr("title");
                    arrow = (arrow.length == 0) ? "" : '<img src="' + arrow + '" align="left" /> ';
                    var sText = $(currentOption).text();
                    var sValue = $(currentOption).val();
                    var sEnabledClass = ($(currentOption).attr("disabled") == true) ? "disabled" : "enabled";
                    sEnabledClass += ' ' + $(currentOption).attr('class');
                    a_array[aid] = { html: arrow + sText, value: sValue, text: sText, index: currentOption.index, id: aid };
                    var innerStyle = getOptionsProperties(currentOption);
                    if (matchIndex(currentOption.index) == true) {
                        aTag += '<a href="javascript:void(0);" class="selected ' + sEnabledClass + '"';
                    } else {
                        aTag += '<a  href="javascript:void(0);" class="' + sEnabledClass + '"';
                    };
                    if (innerStyle != false)
                        aTag += ' style="' + innerStyle + '"';
                    aTag += ' id="' + aid + '">';
                    aTag += arrow + sText + '</a>';
                };
            });
            return aTag;
        };
        function createChildDiv() {
            var id = getPostID("postID");
            var childid = getPostID("postChildID");
            var sStyle = options.style;
            sDiv = "";
            sDiv += '<div id="' + childid + '" class="' + styles.ddChild + '"';
            if (!ddList) {
                sDiv += (sStyle != "") ? ' style="' + sStyle + '"' : '';
            } else {
                sDiv += (sStyle != "") ? ' style="border-top:1px solid #c3c3c3;display:block;position:relative;' + sStyle + '"' : '';
            }
            sDiv += '>';
            return sDiv;
        };

        function createTitleDiv() {
            var titleid = getPostID("postTitleID");
            var arrowid = getPostID("postArrowID");
            var titletextid = getPostID("postTitleTextID");
            var inputhidden = getPostID("postInputhidden");
            var sText = $("#" + elementid + " option:selected").text();
            //custom update by tg, needed after jquery, jquery ui update
            var arrow = "";
            if(getElement(elementid).options.length>0) {
                arrow = $("#"+elementid+" option:selected").prop("title");
            }
            //var arrow = $("#" + elementid + " option:selected").attr("title");
            arrow = (arrow.length == 0 || arrow == undefined || options.showIcon == false) ? "" : '<img src="' + arrow + '" align="left" /> ';
            var sDiv = '<div id="' + titleid + '" class="' + styles.ddTitle + '"';
            sDiv += '>';
            sDiv += '<span id="' + arrowid + '" class="' + styles.arrow + '"></span><span class="textTitle" id="' + titletextid + '">' + arrow + sText + '</span></div>';
            return sDiv;
        };
        function createDropDown() {
            var changeInsertionPoint = false;
            var id = getPostID("postID");
            var titleid = getPostID("postTitleID");
            var titletextid = getPostID("postTitleTextID");
            var childid = getPostID("postChildID");
            var arrowid = getPostID("postArrowID");
            var iWidth = $("#" + elementid).width();
            var sStyle = options.style;
            if ($("#" + id).length > 0) {
                $("#" + id).remove();
                changeInsertionPoint = true;
            }
            var sDiv = '<div id="' + id + '" class="' + styles.dd + '"';
            sDiv += (sStyle != "") ? ' style="' + sStyle + '"' : '';
            sDiv += '>';
            //create title bar
            if (!ddList)
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
            }
            $("#" + id).css("width", iWidth + "px");
            $("#" + childid).css("width", (iWidth - 2) + "px");
            if (allOptions.length > options.visibleRows) {
                var margin = parseInt($("#" + childid + " a:first").css("padding-bottom")) + parseInt($("#" + childid + " a:first").css("padding-top"));
                var iHeight = ((options.rowHeight) * options.visibleRows) - margin;
                $("#" + childid).css("height", iHeight + "px");
            }
            //set out of vision
            if (changeInsertionPoint == false) {
                setOutOfVision();
                addNewEvents(elementid);
            }
            if ($("#" + elementid).attr("disabled") == true) {
                $("#" + id).css("opacity", styles.disbaled);
            } else {
                applyEvents();
                //add events
                //arrow hightlight
                if (!ddList) {
                    $("#" + titleid).bind("mouseover", function(event) {
                        hightlightArrow(1);
                    });
                    $("#" + titleid).bind("mouseout", function(event) {
                        hightlightArrow(0);
                    });
                };
                //open close events
                $("#" + childid + " a.enabled").bind("click", function(event) {
                    event.preventDefault();
                    manageSelection(this);
                    if (!ddList) {
                        $("#" + childid).unbind("mouseover");
                        setInsideWindow(false);
                        var sText = (options.showIcon == false) ? $(this).text() : $(this).html();
                        setTitleText(sText);
                        closeMe();
                    };
                    setValue();
                    //actionSettings.oldIndex = a_array[$(this).attr("id")].index;
                });
                $("#" + childid + " a.disabled").css("opacity", styles.disbaled);
                if (ddList) {
                    $("#" + childid).bind("mouseover", function(event) {
                        if (!actionSettings.keyboardAction) {
                            actionSettings.keyboardAction = true;
                            $(document).bind("keydown", function(event) {
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
                $("#" + childid).bind("mouseout", function(event) { setInsideWindow(false); $(document).unbind("keydown"); actionSettings.keyboardAction = false; actionSettings.currentKey = null; });
                if (!ddList) {
                    $("#" + titleid).bind("click", function(event) {
                        setInsideWindow(false);
                        if ($("#" + childid + ":visible").length == 1) {
                            $("#" + childid).unbind("mouseover");
                        } else {
                            $("#" + childid).bind("mouseover", function(event) { setInsideWindow(true); });
                            openMe();
                        };
                    });
                };
                $("#" + titleid).bind("mouseout", function(evt) {
                    setInsideWindow(false);
                })
            };
        };
        function getByIndex(index) {
            for (var i in a_array) {
                if (a_array[i].index == index) {
                    return a_array[i];
                }
            }
        }
        function manageSelection(obj) {
            var childid = getPostID("postChildID");
            if (!ddList) {
                $("#" + childid + " a.selected").removeClass("selected");
            }
            var selectedA = $("#" + childid + " a.selected").attr("id");
            if (selectedA != undefined) {
                var oldIndex = (actionSettings.oldIndex == undefined || actionSettings.oldIndex == null) ? a_array[selectedA].index : actionSettings.oldIndex;
            };
            if (obj && !ddList) {
                $(obj).addClass("selected");
            };
            if (ddList) {
                var keyCode = actionSettings.currentKey;
                if ($("#" + elementid).attr("multiple") == true) {
                    if (keyCode == 17) {
                        //control
                        actionSettings.oldIndex = a_array[$(obj).attr("id")].index;
                        $(obj).toggleClass("selected");
                        //multiple
                    } else if (keyCode == 16) {
                        $("#" + childid + " a.selected").removeClass("selected");
                        $(obj).addClass("selected");
                        //shift
                        var currentSelected = $(obj).attr("id");
                        var currentIndex = a_array[currentSelected].index;
                        for (var i = Math.min(oldIndex, currentIndex); i <= Math.max(oldIndex, currentIndex); i++) {
                            $("#" + getByIndex(i).id).addClass("selected");
                        }
                    } else {
                        $("#" + childid + " a.selected").removeClass("selected");
                        $(obj).addClass("selected");
                        actionSettings.oldIndex = a_array[$(obj).attr("id")].index;
                    };
                } else {
                    $("#" + childid + " a.selected").removeClass("selected");
                    $(obj).addClass("selected");
                    actionSettings.oldIndex = a_array[$(obj).attr("id")].index;
                };
            };
        };
        function addNewEvents(id) {
            document.getElementById(id).refresh = function(e) {
                $("#" + this.id).dd(options);
            };
        };
        function setInsideWindow(val) {
            actionSettings.insideWindow = val;
        };
        function getInsideWindow() {
            return actionSettings.insideWindow;
        };
        function applyEvents() {
            var mainid = getPostID("postID");
            var actions_array = attributes.actions.split(",");
            for (var iCount = 0; iCount < actions_array.length; iCount++) {
                var action = actions_array[iCount];
                var actionFound = $("#" + elementid).attr(action);
                if (actionFound != undefined) {
                    switch (action) {
                        case "onfocus":
                            $("#" + mainid).bind("mouseenter", function(event) {
                                document.getElementById(elementid).focus();
                            });
                            break;
                        case "onclick":
                            $("#" + mainid).bind("click", function(event) {
                                document.getElementById(elementid).onclick();
                            });
                            break;
                        case "ondblclick":
                            $("#" + mainid).bind("dblclick", function(event) {
                                document.getElementById(elementid).ondblclick();
                            });
                            break;
                        case "onmousedown":
                            $("#" + mainid).bind("mousedown", function(event) {
                                document.getElementById(elementid).onmousedown();
                            });
                            break;
                        case "onmouseup":
                            //has in closeMe mthod
                            $("#" + mainid).bind("mouseup", function(event) {
                                document.getElementById(elementid).onmouseup();
                                //setValue();
                            });
                            break;
                        case "onmouseover":
                            $("#" + mainid).bind("mouseover", function(event) {
                                document.getElementById(elementid).onmouseover();
                            });
                            break;
                        case "onmousemove":
                            $("#" + mainid).bind("mousemove", function(event) {
                                document.getElementById(elementid).onmousemove();
                            });
                            break;
                        case "onmouseout":
                            $("#" + mainid).bind("mouseout", function(event) {
                                document.getElementById(elementid).onmouseout();
                            });
                            break;
                    };
                };
            };

        };
        function setOutOfVision() {
            var sId = getPostID("postElementHolder");
            $("#" + elementid).after("<div style='height:0px;overflow:hidden;position:absolute;' id='" + sId + "'></div>");
            $("#" + elementid).appendTo($("#" + sId));
        };
        function setTitleText(sText) {
            var titletextid = getPostID("postTitleTextID");
            $("#" + titletextid).html(sText);
        };
        function next() {
            var titletextid = getPostID("postTitleTextID");
            var childid = getPostID("postChildID");
            var allAs = $("#" + childid + " a.enabled");
            for (var current = 0; current < allAs.length; current++) {
                var currentA = allAs[current];
                var id = $(currentA).attr("id");
                if ($(currentA).hasClass("selected") && current < allAs.length - 1) {
                    $("#" + childid + " a.selected").removeClass("selected");
                    $(allAs[current + 1]).addClass("selected");
                    //manageSelection(allAs[current+1]);
                    var selectedA = $("#" + childid + " a.selected").attr("id");
                    if (!ddList) {
                        var sText = (options.showIcon == false) ? a_array[selectedA].text : a_array[selectedA].html;
                        setTitleText(sText);
                    }
                    if (parseInt(($("#" + selectedA).position().top + $("#" + selectedA).height())) >= parseInt($("#" + childid).height())) {
                        $("#" + childid).scrollTop(($("#" + childid).scrollTop()) + $("#" + selectedA).height() + $("#" + selectedA).height());
                    };
                    break;
                };
            };
        };
        function previous() {
            var titletextid = getPostID("postTitleTextID");
            var childid = getPostID("postChildID");
            var allAs = $("#" + childid + " a.enabled");
            for (var current = 0; current < allAs.length; current++) {
                var currentA = allAs[current];
                var id = $(currentA).attr("id");
                if ($(currentA).hasClass("selected") && current != 0) {
                    $("#" + childid + " a.selected").removeClass("selected");
                    $(allAs[current - 1]).addClass("selected");
                    //manageSelection(allAs[current-1]);
                    var selectedA = $("#" + childid + " a.selected").attr("id");
                    if (!ddList) {
                        var sText = (options.showIcon == false) ? a_array[selectedA].text : a_array[selectedA].html;
                        setTitleText(sText);
                    }
                    if (parseInt(($("#" + selectedA).position().top + $("#" + selectedA).height())) <= 0) {
                        $("#" + childid).scrollTop(($("#" + childid).scrollTop() - $("#" + childid).height()) - $("#" + selectedA).height());
                    };
                    break;
                };
            };
        };
        function setValue() {
            var childid = getPostID("postChildID");
            var allSelected = $("#" + childid + " a.selected");
            if (allSelected.length == 1) {
                var sText = $("#" + childid + " a.selected").text();
                var selectedA = $("#" + childid + " a.selected").attr("id");
                if (selectedA != undefined) {
                    var sValue = a_array[selectedA].value;
                    document.getElementById(elementid).selectedIndex = a_array[selectedA].index;
                };
            } else if (allSelected.length > 1) {
                var alls = $("#" + elementid + " > option:selected").removeAttr("selected");
                for (var i = 0; i < allSelected.length; i++) {
                    var selectedA = $(allSelected[i]).attr("id");
                    var index = a_array[selectedA].index;
                    document.getElementById(elementid).options[index].selected = "selected";
                };
            };
        };
        function openMe() {
            var childid = getPostID("postChildID");
            if (oldDiv != "" && childid != oldDiv) {
                $("#" + oldDiv).slideUp("fast");
                $("#" + oldDiv).css({ zIndex: '0' });
            };
            if ($("#" + childid).css("display") == "none") {
                selectedValue = a_array[$("#" + childid + " a.selected").attr("id")].text;
                $(document).bind("keydown", function(event) {
                    var keyCode = event.keyCode;
                    if (keyCode == 39 || keyCode == 40) {
                        //move to next
                        event.preventDefault(); event.stopPropagation();
                        next();
                    };
                    if (keyCode == 37 || keyCode == 38) {
                        event.preventDefault(); event.stopPropagation();
                        //move to previous
                        previous();
                    };
                    if (keyCode == 27 || keyCode == 13) {
                        closeMe();
                        setValue();
                    };
                    if ($("#" + elementid).attr("onkeydown") != undefined) {
                        document.getElementById(elementid).onkeydown();
                    };
                });
                $(document).bind("keyup", function(event) {
                    if ($("#" + elementid).attr("onkeyup") != undefined) {
                        //$("#"+elementid).keyup();
                        document.getElementById(elementid).onkeyup();
                    };
                });

                $(document).bind("mouseup", function(evt) {
                    if (getInsideWindow() == false) {
                        closeMe();
                    }
                });
                $("#" + childid).css({ zIndex: options.zIndex });
                $("#" + childid).slideDown("fast");
                if (childid != oldDiv) {
                    oldDiv = childid;
                }
            };
        };
        function closeMe() {
            var childid = getPostID("postChildID");
            $(document).unbind("keydown");
            $(document).unbind("keyup");
            $(document).unbind("mouseup");
            $("#" + childid).slideUp("fast", function(event) {
                checkMethodAndApply();
                $("#" + childid).css({ zIndex: '0' });
            });

        };
        function checkMethodAndApply() {
            var childid = getPostID("postChildID");
            if ($("#" + elementid).attr("onchange") != undefined) {
                var currentSelectedValue = a_array[$("#" + childid + " a.selected").attr("id")].text;
                if (selectedValue != currentSelectedValue) { document.getElementById(elementid).onchange(); };
            }
            if ($("#" + elementid).attr("onmouseup") != undefined) {
                document.getElementById(elementid).onmouseup();
            }
            if ($("#" + elementid).attr("onblur") != undefined) {
                $(document).bind("mouseup", function(evt) {
                    $("#" + elementid).focus();
                    $("#" + elementid)[0].blur();
                    setValue();
                    $(document).unbind("mouseup");
                });
            };
        };
        function hightlightArrow(ison) {
            var arrowid = getPostID("postArrowID");
            if (ison == 1)
                $("#" + arrowid).css({ backgroundPosition: '0 100%' });
            else
                $("#" + arrowid).css({ backgroundPosition: '0 0' });
        };
    };
    $.fn.msDropDown = function(properties) {
        var dds = $(this);
        for (var iCount = 0; iCount < dds.length; iCount++) {
            var id = $(dds[iCount]).attr("id");
            if (properties == undefined) {
                $("#" + id).dd();
            } else {
                $("#" + id).dd(properties);
            };
        };
    };
})(jQuery);