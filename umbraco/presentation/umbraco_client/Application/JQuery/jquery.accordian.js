// 
// jQuery UI Accordion 1.6
//  
// Copyright (c) 2007 JÃƒÂ¶rn Zaefferer
// 
// http://docs.jquery.com/UI/Accordion
// 
// Dual licensed under the MIT and GPL licenses:
//    http://www.opensource.org/licenses/mit-license.php
//    http://www.gnu.org/licenses/gpl.html
// 
//  Revision: $Id: jquery.accordion.js 4876 2008-03-08 11:49:04Z joern.zaefferer $
// 
// 

 (function($) {

    // If the UI scope is not available, add it
    $.ui = $.ui || {};

    $.fn.extend({
        accordion: function(options, data) {
            var args = Array.prototype.slice.call(arguments, 1);

            return this.each(function() {
                if (typeof options == "string") {
                    var accordion = $.data(this, "ui-accordion");
                    accordion[options].apply(accordion, args);
                    // INIT with optional options
                } else if (!$(this).is(".ui-accordion"))
                    $.data(this, "ui-accordion", new $.ui.accordion(this, options));
            });
        },
        // deprecated, use accordion("activate", index) instead
        activate: function(index) {
            return this.accordion("activate", index);
        }
    });

    $.ui.accordion = function(container, options) {

        // setup configuration
        this.options = options = $.extend({}, $.ui.accordion.defaults, options);
        this.element = container;

        $(container).addClass("ui-accordion");

        if (options.navigation) {
            var current = $(container).find("a").filter(options.navigationFilter);
            if (current.length) {
                if (current.filter(options.header).length) {
                    options.active = current;
                } else {
                    options.active = current.parent().parent().prev();
                    current.addClass("current");
                }
            }
        }

        // calculate active if not specified, using the first header
        options.headers = $(container).find(options.header);
        options.active = findActive(options.headers, options.active);

        if (options.fillSpace) {
            var maxHeight = $(container).parent().height();
            options.headers.each(function() {
                maxHeight -= $(this).outerHeight();
            });
            var maxPadding = 0;
            options.headers.next().each(function() {
                maxPadding = Math.max(maxPadding, $(this).innerHeight() - $(this).height());
            }).height(maxHeight - maxPadding);
        } else if (options.autoheight) {
            var maxHeight = 0;
            options.headers.next().each(function() {
                maxHeight = Math.max(maxHeight, $(this).outerHeight());
            }).height(maxHeight);
        }

        options.headers
		.not(options.active || "")
		.next()
		.hide();
        options.active.parent().andSelf().addClass(options.selectedClass);

        if (options.event)
            $(container).bind((options.event) + ".ui-accordion", clickHandler);
    };

    $.ui.accordion.prototype = {
        activate: function(index) {
            // call clickHandler with custom event
            clickHandler.call(this.element, {
                target: findActive(this.options.headers, index)[0]
            });
        },

        enable: function() {
            this.options.disabled = false;
        },
        disable: function() {
            this.options.disabled = true;
        },
        destroy: function() {
            this.options.headers.next().css("display", "");
            if (this.options.fillSpace || this.options.autoheight) {
                this.options.headers.next().css("height", "");
            }
            $.removeData(this.element, "ui-accordion");
            $(this.element).removeClass("ui-accordion").unbind(".ui-accordion");
        }
    }

    function scopeCallback(callback, scope) {
        return function() {
            return callback.apply(scope, arguments);
        };
    }

    function completed(cancel) {
        // if removed while animated data can be empty
        if (!$.data(this, "ui-accordion"))
            return;
        var instance = $.data(this, "ui-accordion");
        var options = instance.options;
        options.running = cancel ? 0 : --options.running;
        if (options.running)
            return;
        if (options.clearStyle) {
            options.toShow.add(options.toHide).css({
                height: "",
                overflow: ""
            });
        }
        $(this).triggerHandler("change.ui-accordion", [options.data], options.change);
    }

    function toggle(toShow, toHide, data, clickedActive, down) {
        var options = $.data(this, "ui-accordion").options;
        options.toShow = toShow;
        options.toHide = toHide;
        options.data = data;
        var complete = scopeCallback(completed, this);

        // count elements to animate
        options.running = toHide.size() == 0 ? toShow.size() : toHide.size();

        if (options.animated) {
            if (!options.alwaysOpen && clickedActive) {
                $.ui.accordion.animations[options.animated]({
                    toShow: jQuery([]),
                    toHide: toHide,
                    complete: complete,
                    down: down,
                    autoheight: options.autoheight
                });
            } else {
                $.ui.accordion.animations[options.animated]({
                    toShow: toShow,
                    toHide: toHide,
                    complete: complete,
                    down: down,
                    autoheight: options.autoheight
                });
            }
        } else {
            if (!options.alwaysOpen && clickedActive) {
                toShow.toggle();
            } else {
                toHide.hide();
                toShow.show();
            }
            complete(true);
        }
    }

    function clickHandler(event) {
        var options = $.data(this, "ui-accordion").options;
        if (options.disabled)
            return false;

        // called only when using activate(false) to close all parts programmatically
        if (!event.target && !options.alwaysOpen) {
            options.active.parent().andSelf().toggleClass(options.selectedClass);
            var toHide = options.active.next(),
			data = {
			    instance: this,
			    options: options,
			    newHeader: jQuery([]),
			    oldHeader: options.active,
			    newContent: jQuery([]),
			    oldContent: toHide
			},
			toShow = options.active = $([]);
            toggle.call(this, toShow, toHide, data);
            return false;
        }
        // get the click target
        var clicked = $(event.target);

        // due to the event delegation model, we have to check if one
        // of the parent elements is our actual header, and find that
        if (clicked.parents(options.header).length)
            while (!clicked.is(options.header))
            clicked = clicked.parent();

        var clickedActive = clicked[0] == options.active[0];

        // if animations are still active, or the active header is the target, ignore click
        if (options.running || (options.alwaysOpen && clickedActive))
            return false;
        if (!clicked.is(options.header))
            return;

        // switch classes
        options.active.parent().andSelf().toggleClass(options.selectedClass);
        if (!clickedActive) {
            clicked.parent().andSelf().addClass(options.selectedClass);
        }

        // find elements to show and hide
        var toShow = clicked.next(),
		toHide = options.active.next(),
        //data = [clicked, options.active, toShow, toHide],
		data = {
		    instance: this,
		    options: options,
		    newHeader: clicked,
		    oldHeader: options.active,
		    newContent: toShow,
		    oldContent: toHide
		},
		down = options.headers.index(options.active[0]) > options.headers.index(clicked[0]);

        options.active = clickedActive ? $([]) : clicked;
        toggle.call(this, toShow, toHide, data, clickedActive, down);

        return false;
    };

    function findActive(headers, selector) {
        return selector != undefined
		? typeof selector == "number"
			? headers.filter(":eq(" + selector + ")")
			: headers.not(headers.not(selector))
		: selector === false
			? $([])
			: headers.filter(":eq(0)");
    }

    $.extend($.ui.accordion, {
        defaults: {
            selectedClass: "selected",
            alwaysOpen: true,
            animated: 'slide',
            event: "click",
            header: "a",
            autoheight: true,
            running: 0,
            navigationFilter: function() {
                return this.href.toLowerCase() == location.href.toLowerCase();
            }
        },
        animations: {
            slide: function(options, additions) {
                options = $.extend({
                    easing: "swing",
                    duration: 300
                }, options, additions);
                if (!options.toHide.size()) {
                    options.toShow.animate({ height: "show" }, options);
                    return;
                }
                var hideHeight = options.toHide.height(),
				showHeight = options.toShow.height(),
				difference = showHeight / hideHeight;
                options.toShow.css({ height: 0, overflow: 'hidden' }).show();
                options.toHide.filter(":hidden").each(options.complete).end().filter(":visible").animate({ height: "hide" }, {
                    step: function(now) {
                        var current = (hideHeight - now) // difference;
                        if ($.browser.msie || $.browser.opera) {
                            current = Math.ceil(current);
                        }
                        options.toShow.height(current);
                    },
                    duration: options.duration,
                    easing: options.easing,
                    complete: function() {
                        if (!options.autoheight) {
                            options.toShow.css("height", "auto");
                        }

                        options.toShow.css("overflow", "auto");
                        
                        options.complete();
                    }
                });
            },
            bounceslide: function(options) {
                this.slide(options, {
                    easing: options.down ? "bounceout" : "swing",
                    duration: options.down ? 1000 : 200
                });
            },
            easeslide: function(options) {
                this.slide(options, {
                    easing: "easeinout",
                    duration: 700
                })
            }
        }
    });

})(jQuery);
