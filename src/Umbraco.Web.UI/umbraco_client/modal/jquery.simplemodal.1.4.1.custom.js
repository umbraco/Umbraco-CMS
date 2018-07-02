
; (function ($) {
    var ie6 = $.browser && $.browser.msie && parseInt($.browser.version) === 6 && typeof window['XMLHttpRequest'] !== 'object',
		ie7 = $.browser && $.browser.msie && parseInt($.browser.version) === 7,
		ieQuirks = null,
		w = [];

    /*
    * Create and display a modal dialog.
    *
    * @param {string, object} data A string, jQuery object or DOM object
    * @param {object} [options] An optional object containing options overrides
    */
    $.fullmodal = function (data, options) {
        return $.fullmodal.impl.init(data, options);
    };

    /*
    * Close the modal dialog.
    */
    $.fullmodal.close = function () {
        $.fullmodal.impl.close();
    };

    /*
    * Set focus on first or last visible input in the modal dialog. To focus on the last
    * element, call $.fullmodal.focus('last'). If no input elements are found, focus is placed
    * on the data wrapper element.
    */
    $.fullmodal.focus = function (pos) {
        $.fullmodal.impl.focus(pos);
    };

    /*
    * Determine and set the dimensions of the modal dialog container.
    * setPosition() is called if the autoPosition option is true.
    */
    $.fullmodal.setContainerDimensions = function () {
        $.fullmodal.impl.setContainerDimensions();
    };

    /*
    * Re-position the modal dialog.
    */
    $.fullmodal.setPosition = function () {
        $.fullmodal.impl.setPosition();
    };

    /*
    * Update the modal dialog. If new dimensions are passed, they will be used to determine
    * the dimensions of the container.
    *
    * setContainerDimensions() is called, which in turn calls setPosition(), if enabled.
    * Lastly, focus() is called is the focus option is true.
    */
    $.fullmodal.update = function (height, width) {
        $.fullmodal.impl.update(height, width);
    };

    /*
    * Chained function to create a modal dialog.
    *
    * @param {object} [options] An optional object containing options overrides
    */
    $.fn.fullmodal = function (options) {
        return $.fullmodal.impl.init(this, options);
    };

    /*
    * SimpleModal default options
    *
    * appendTo:		(String:'body') The jQuery selector to append the elements to. For .NET, use 'form'.
    * focus:			(Boolean:true) Focus in the first visible, enabled element?
    * opacity:			(Number:50) The opacity value for the overlay div, from 0 - 100
    * overlayId:		(String:'fullmodal-overlay') The DOM element id for the overlay div
    * overlayCss:		(Object:{}) The CSS styling for the overlay div
    * containerId:		(String:'fullmodal-container') The DOM element id for the container div
    * containerCss:	(Object:{}) The CSS styling for the container div
    * dataId:			(String:'fullmodal-data') The DOM element id for the data div
    * dataCss:			(Object:{}) The CSS styling for the data div
    * minHeight:		(Number:null) The minimum height for the container
    * minWidth:		(Number:null) The minimum width for the container
    * maxHeight:		(Number:null) The maximum height for the container. If not specified, the window height is used.
    * maxWidth:		(Number:null) The maximum width for the container. If not specified, the window width is used.
    * autoResize:		(Boolean:false) Automatically resize the container if it exceeds the browser window dimensions?
    * autoPosition:	(Boolean:true) Automatically position the container upon creation and on window resize?
    * zIndex:			(Number: 1000) Starting z-index value
    * close:			(Boolean:true) If true, closeHTML, escClose and overClose will be used if set.
    If false, none of them will be used.
    * closeHTML:		(String:'<a class="modalCloseImg" title="Close"></a>') The HTML for the default close link.
    SimpleModal will automatically add the closeClass to this element.
    * closeClass:		(String:'fullmodal-close') The CSS class used to bind to the close event
    * escClose:		(Boolean:true) Allow Esc keypress to close the dialog?
    * overlayClose:	(Boolean:false) Allow click on overlay to close the dialog?
    * position:		(Array:null) Position of container [top, left]. Can be number of pixels or percentage
    * persist:			(Boolean:false) Persist the data across modal calls? Only used for existing
    DOM elements. If true, the data will be maintained across modal calls, if false,
    the data will be reverted to its original state.
    * modal:			(Boolean:true) User will be unable to interact with the page below the modal or tab away from the dialog.
    If false, the overlay, iframe, and certain events will be disabled allowing the user to interact
    with the page below the dialog.
    * onOpen:			(Function:null) The callback function used in place of SimpleModal's open
    * onShow:			(Function:null) The callback function used after the modal dialog has opened
    * onClose:			(Function:null) The callback function used in place of SimpleModal's close
    */
    $.fullmodal.defaults = {
        appendTo: 'body',
        focus: true,
        opacity: 50,
        overlayId: 'fullmodal-overlay',
        overlayCss: {},
        containerId: 'fullmodal-container',
        containerCss: {},
        dataId: 'fullmodal-data',
        dataCss: {},
        minHeight: null,
        minWidth: null,
        maxHeight: null,
        maxWidth: null,
        autoResize: false,
        autoPosition: true,
        zIndex: 1000,
        close: true,
        closeHTML: '<a class="modalCloseImg" title="Close"></a>',
        closeClass: 'fullmodal-close',
        escClose: true,
        overlayClose: false,
        position: null,
        persist: false,
        modal: true,
        onOpen: null,
        onShow: null,
        onClose: null
    };

    /*
    * Main modal object
    * o = options
    */
    $.fullmodal.impl = {
        /*
        * Contains the modal dialog elements and is the object passed
        * back to the callback (onOpen, onShow, onClose) functions
        */
        d: {},
        /*
        * Initialize the modal dialog
        */
        init: function (data, options) {
            var s = this;

            // don't allow multiple calls
            if (s.d.data) {
                return false;
            }

            // $.boxModel is undefined if checked earlier
            ieQuirks = $.browser.msie && !$.boxModel;

            // merge defaults and user options
            s.o = $.extend({}, $.fullmodal.defaults, options);

            // keep track of z-index
            s.zIndex = s.o.zIndex;

            // set the onClose callback flag
            s.occb = false;

            // determine how to handle the data based on its type
            if (typeof data === 'object') {
                // convert DOM object to a jQuery object
                data = data instanceof jQuery ? data : $(data);
                s.d.placeholder = false;

                // if the object came from the DOM, keep track of its parent
                if (data.parent().parent().size() > 0) {
                    data.before($('<span></span>')
						.attr('id', 'fullmodal-placeholder')
						.css({ display: 'none' }));

                    s.d.placeholder = true;
                    s.display = data.css('display');

                    // persist changes? if not, make a clone of the element
                    if (!s.o.persist) {
                        s.d.orig = data.clone(true);
                    }
                }
            }
            else if (typeof data === 'string' || typeof data === 'number') {
                // just insert the data as innerHTML
                data = $('<div></div>').html(data);
            }
            else {
                // unsupported data type!
                alert('SimpleModal Error: Unsupported data type: ' + typeof data);
                return s;
            }

            // create the modal overlay, container and, if necessary, iframe
            s.create(data);
            data = null;

            // display the modal dialog
            s.open();

            // useful for adding events/manipulating data in the modal dialog
            if ($.isFunction(s.o.onShow)) {
                s.o.onShow.apply(s, [s.d]);
            }

            // don't break the chain =)
            return s;
        },
        /*
        * Create and add the modal overlay and container to the page
        */
        create: function (data) {
            var s = this;

            // get the window properties
            w = s.getDimensions();

            // add an iframe to prevent select options from bleeding through
            if (s.o.modal && ie6) {
                s.d.iframe = $('<iframe src="javascript:false;"></iframe>')
					.css($.extend(s.o.iframeCss, {
					    display: 'none',
					    opacity: 0,
					    position: 'fixed',
					    height: w[0],
					    width: w[1],
					    zIndex: s.o.zIndex,
					    top: 0,
					    left: 0
					}))
					.appendTo(s.o.appendTo);
            }

            // create the overlay
            s.d.overlay = $('<div></div>')
				.attr('id', s.o.overlayId)
				.addClass('fullmodal-overlay')
				.css($.extend(s.o.overlayCss, {
				    display: 'none',
				    opacity: s.o.opacity / 100,
				    height: s.o.modal ? w[0] : 0,
				    width: s.o.modal ? w[1] : 0,
				    position: 'fixed',
				    left: 0,
				    top: 0,
				    zIndex: s.o.zIndex + 1
				}))
				.appendTo(s.o.appendTo);

            // create the container
            s.d.container = $('<div></div>')
				.attr('id', s.o.containerId)
				.addClass('fullmodal-container')
				.css($.extend(s.o.containerCss, {
				    display: 'none',
				    position: 'fixed',
				    zIndex: s.o.zIndex + 2
				}))
				.append(s.o.close && s.o.closeHTML
					? $(s.o.closeHTML).addClass(s.o.closeClass)
					: '')
				.appendTo(s.o.appendTo);

            s.d.wrap = $('<div></div>')
				.attr('tabIndex', -1)
				.addClass('fullmodal-wrap')
				.css({ height: '100%', outline: 0, width: '100%' })
				.appendTo(s.d.container);

            // add styling and attributes to the data
            // append to body to get correct dimensions, then move to wrap
            s.d.data = data
				.attr('id', data.attr('id') || s.o.dataId)
				.addClass('fullmodal-data')
				.css($.extend(s.o.dataCss, {
				    display: 'none'
				}))
				.appendTo('body');
            data = null;

            s.setContainerDimensions();
            s.d.data.appendTo(s.d.wrap);

            // fix issues with IE
            if (ie6 || ieQuirks) {
                s.fixIE();
            }
        },
        /*
        * Bind events
        */
        bindEvents: function () {
            var s = this;

            // bind the close event to any element with the closeClass class
            $('.' + s.o.closeClass).bind('click.fullmodal', function (e) {
                e.preventDefault();
                s.close();
            });

            // bind the overlay click to the close function, if enabled
            if (s.o.modal && s.o.close && s.o.overlayClose) {
                s.d.overlay.bind('click.fullmodal', function (e) {
                    e.preventDefault();
                    s.close();
                });
            }

            // bind keydown events
            $(document).bind('keydown.fullmodal', function (e) {
                if (s.o.modal && e.keyCode === 9) { // TAB
                    s.watchTab(e);
                }
                else if ((s.o.close && s.o.escClose) && e.keyCode === 27) { // ESC
                    e.preventDefault();
                    s.close();
                }
            });

            // update window size
            $(window).bind('resize.fullmodal', function () {
                // redetermine the window width/height
                w = s.getDimensions();

                // reposition the dialog
                s.o.autoResize ? s.setContainerDimensions() : s.o.autoPosition && s.setPosition();

                if (ie6 || ieQuirks) {
                    s.fixIE();
                }
                else if (s.o.modal) {
                    // update the iframe & overlay
                    s.d.iframe && s.d.iframe.css({ height: w[0], width: w[1] });
                    s.d.overlay.css({ height: w[0], width: w[1] });
                }
            });
        },
        /*
        * Unbind events
        */
        unbindEvents: function () {
            $('.' + this.o.closeClass).unbind('click.fullmodal');
            $(document).unbind('keydown.fullmodal');
            $(window).unbind('resize.fullmodal');
            this.d.overlay.unbind('click.fullmodal');
        },
        /*
        * Fix issues in IE6 and IE7 in quirks mode
        */
        fixIE: function () {
            var s = this, p = s.o.position;

            // simulate fixed position - adapted from BlockUI
            $.each([s.d.iframe || null, !s.o.modal ? null : s.d.overlay, s.d.container], function (i, el) {
                if (el) {
                    var bch = 'document.body.clientHeight', bcw = 'document.body.clientWidth',
						bsh = 'document.body.scrollHeight', bsl = 'document.body.scrollLeft',
						bst = 'document.body.scrollTop', bsw = 'document.body.scrollWidth',
						ch = 'document.documentElement.clientHeight', cw = 'document.documentElement.clientWidth',
						sl = 'document.documentElement.scrollLeft', st = 'document.documentElement.scrollTop',
						s = el[0].style;

                    s.position = 'absolute';
                    if (i < 2) {
                        s.removeExpression('height');
                        s.removeExpression('width');
                        s.setExpression('height', '' + bsh + ' > ' + bch + ' ? ' + bsh + ' : ' + bch + ' + "px"');
                        s.setExpression('width', '' + bsw + ' > ' + bcw + ' ? ' + bsw + ' : ' + bcw + ' + "px"');
                    }
                    else {
                        var te, le;
                        if (p && p.constructor === Array) {
                            var top = p[0]
								? typeof p[0] === 'number' ? p[0].toString() : p[0].replace(/px/, '')
								: el.css('top').replace(/px/, '');
                            te = top.indexOf('%') === -1
								? top + ' + (t = ' + st + ' ? ' + st + ' : ' + bst + ') + "px"'
								: parseInt(top.replace(/%/, '')) + ' * ((' + ch + ' || ' + bch + ') / 100) + (t = ' + st + ' ? ' + st + ' : ' + bst + ') + "px"';

                            if (p[1]) {
                                var left = typeof p[1] === 'number' ? p[1].toString() : p[1].replace(/px/, '');
                                le = left.indexOf('%') === -1
									? left + ' + (t = ' + sl + ' ? ' + sl + ' : ' + bsl + ') + "px"'
									: parseInt(left.replace(/%/, '')) + ' * ((' + cw + ' || ' + bcw + ') / 100) + (t = ' + sl + ' ? ' + sl + ' : ' + bsl + ') + "px"';
                            }
                        }
                        else {
                            te = '(' + ch + ' || ' + bch + ') / 2 - (this.offsetHeight / 2) + (t = ' + st + ' ? ' + st + ' : ' + bst + ') + "px"';
                            le = '(' + cw + ' || ' + bcw + ') / 2 - (this.offsetWidth / 2) + (t = ' + sl + ' ? ' + sl + ' : ' + bsl + ') + "px"';
                        }
                        s.removeExpression('top');
                        s.removeExpression('left');
                        s.setExpression('top', te);
                        s.setExpression('left', le);
                    }
                }
            });
        },
        /*
        * Place focus on the first or last visible input
        */
        focus: function (pos) {
            var s = this, p = pos && $.inArray(pos, ['first', 'last']) !== -1 ? pos : 'first';

            // focus on dialog or the first visible/enabled input element
            var input = $(':input:enabled:visible:' + p, s.d.wrap);
            setTimeout(function () {
                input.length > 0 ? input.focus() : s.d.wrap.focus();
            }, 10);
        },
        getDimensions: function () {
            var el = $(window);

            // fix a jQuery/Opera bug with determining the window height
            var h = $.browser && $.browser.opera && $.browser.version > '9.5' && $.fn.jquery < '1.3'
						|| $.browser && $.browser.opera && $.browser.version < '9.5' && $.fn.jquery > '1.2.6'
				? el[0].innerHeight : el.height();

            return [h, el.width()];
        },
        getVal: function (v, d) {
            return v ? (typeof v === 'number' ? v
					: v === 'auto' ? 0
					: v.indexOf('%') > 0 ? ((parseInt(v.replace(/%/, '')) / 100) * (d === 'h' ? w[0] : w[1]))
					: parseInt(v.replace(/px/, '')))
				: null;
        },
        /*
        * Update the container. Set new dimensions, if provided.
        * Focus, if enabled. Re-bind events.
        */
        update: function (height, width) {
            var s = this;

            // prevent update if dialog does not exist
            if (!s.d.data) {
                return false;
            }

            // reset orig values
            s.d.origHeight = s.getVal(height, 'h');
            s.d.origWidth = s.getVal(width, 'w');

            // hide data to prevent screen flicker
            s.d.data.hide();
            height && s.d.container.css('height', height);
            width && s.d.container.css('width', width);
            s.setContainerDimensions();
            s.d.data.show();
            s.o.focus && s.focus();

            // rebind events
            s.unbindEvents();
            s.bindEvents();
        },
        setContainerDimensions: function () {
            var s = this,
				badIE = ie6 || ie7;

            // get the dimensions for the container and data
            var ch = s.d.origHeight ? s.d.origHeight : $.browser.opera ? s.d.container.height() : s.getVal(badIE ? s.d.container[0].currentStyle['height'] : s.d.container.css('height'), 'h'),
				cw = s.d.origWidth ? s.d.origWidth : $.browser.opera ? s.d.container.width() : s.getVal(badIE ? s.d.container[0].currentStyle['width'] : s.d.container.css('width'), 'w'),
				dh = s.d.data.outerHeight(true), dw = s.d.data.outerWidth(true);

            s.d.origHeight = s.d.origHeight || ch;
            s.d.origWidth = s.d.origWidth || cw;

            // mxoh = max option height, mxow = max option width
            var mxoh = s.o.maxHeight ? s.getVal(s.o.maxHeight, 'h') : null,
				mxow = s.o.maxWidth ? s.getVal(s.o.maxWidth, 'w') : null,
				mh = mxoh && mxoh < w[0] ? mxoh : w[0],
				mw = mxow && mxow < w[1] ? mxow : w[1];

            // moh = min option height
            var moh = s.o.minHeight ? s.getVal(s.o.minHeight, 'h') : 'auto';
            if (!ch) {
                if (!dh) { ch = moh; }
                else {
                    if (dh > mh) { ch = mh; }
                    else if (s.o.minHeight && moh !== 'auto' && dh < moh) { ch = moh; }
                    else { ch = dh; }
                }
            }
            else {
                ch = s.o.autoResize && ch > mh ? mh : ch < moh ? moh : ch;
            }

            // mow = min option width
            var mow = s.o.minWidth ? s.getVal(s.o.minWidth, 'w') : 'auto';
            if (!cw) {
                if (!dw) { cw = mow; }
                else {
                    if (dw > mw) { cw = mw; }
                    else if (s.o.minWidth && mow !== 'auto' && dw < mow) { cw = mow; }
                    else { cw = dw; }
                }
            }
            else {
                cw = s.o.autoResize && cw > mw ? mw : cw < mow ? mow : cw;
            }

            s.d.container.css({ height: ch, width: cw });
            s.d.wrap.css({ overflow: (dh > ch || dw > cw) ? 'auto' : 'visible' });
            s.o.autoPosition && s.setPosition();
        },
        setPosition: function () {
            var s = this, top, left,
				hc = (w[0] / 2) - (s.d.container.outerHeight(true) / 2),
				vc = (w[1] / 2) - (s.d.container.outerWidth(true) / 2);

            if (s.o.position && Object.prototype.toString.call(s.o.position) === '[object Array]') {
                top = s.o.position[0] || hc;
                left = s.o.position[1] || vc;
            } else {
                top = hc;
                left = vc;
            }
            s.d.container.css({ left: left, top: top });
        },
        watchTab: function (e) {
            var s = this;

            if ($(e.target).parents('.fullmodal-container').length > 0) {
                // save the list of inputs
                s.inputs = $(':input:enabled:visible:first, :input:enabled:visible:last', s.d.data[0]);

                // if it's the first or last tabbable element, refocus
                if ((!e.shiftKey && e.target === s.inputs[s.inputs.length - 1]) ||
						(e.shiftKey && e.target === s.inputs[0]) ||
						s.inputs.length === 0) {
                    e.preventDefault();
                    var pos = e.shiftKey ? 'last' : 'first';
                    s.focus(pos);
                }
            }
            else {
                // might be necessary when custom onShow callback is used
                e.preventDefault();
                s.focus();
            }
        },
        /*
        * Open the modal dialog elements
        * - Note: If you use the onOpen callback, you must "show" the
        *	        overlay and container elements manually
        *         (the iframe will be handled by SimpleModal)
        */
        open: function () {
            var s = this;
            // display the iframe
            s.d.iframe && s.d.iframe.show();

            if ($.isFunction(s.o.onOpen)) {
                // execute the onOpen callback
                s.o.onOpen.apply(s, [s.d]);
            }
            else {
                // display the remaining elements
                s.d.overlay.show();
                s.d.container.show();
                s.d.data.show();
            }

            s.o.focus && s.focus();

            // bind default events
            s.bindEvents();
        },
        /*
        * Close the modal dialog
        * - Note: If you use an onClose callback, you must remove the
        *         overlay, container and iframe elements manually
        *
        * @param {boolean} external Indicates whether the call to this
        *     function was internal or external. If it was external, the
        *     onClose callback will be ignored
        */
        close: function () {
            var s = this;

            // prevent close when dialog does not exist
            if (!s.d.data) {
                return false;
            }

            // remove the default events
            s.unbindEvents();

            if ($.isFunction(s.o.onClose) && !s.occb) {
                // set the onClose callback flag
                s.occb = true;

                // execute the onClose callback
                s.o.onClose.apply(s, [s.d]);
            }
            else {
                // if the data came from the DOM, put it back
                if (s.d.placeholder) {
                    var ph = $('#fullmodal-placeholder');
                    // save changes to the data?
                    if (s.o.persist) {
                        // insert the (possibly) modified data back into the DOM
                        ph.replaceWith(s.d.data.removeClass('fullmodal-data').css('display', s.display));
                    }
                    else {
                        // remove the current and insert the original,
                        // unmodified data back into the DOM
                        s.d.data.hide().remove();
                        ph.replaceWith(s.d.orig);
                    }
                }
                else {
                    // otherwise, remove it
                    s.d.data.hide().remove();
                }

                // remove the remaining elements
                s.d.container.hide().remove();
                s.d.overlay.hide();
                s.d.iframe && s.d.iframe.hide().remove();
                setTimeout(function () {
                    // opera work-around
                    s.d.overlay.remove();

                    // reset the dialog object
                    s.d = {};
                }, 10);
            }
        }
    };
})(jQuery);
