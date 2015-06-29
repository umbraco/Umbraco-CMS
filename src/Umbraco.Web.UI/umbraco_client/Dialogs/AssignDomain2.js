Umbraco.Sys.registerNamespace("Umbraco.Dialogs");

(function ($) {

    // register AssignDomain dialog
    Umbraco.Dialogs.AssignDomain2 = base2.Base.extend({
        
        _opts: null,
        
        _isRepeated: function (element) {
            var inputs = $('form input.domain');
            var elementName = element.attr('name');
            var repeated = false;
            inputs.each(function() {
                var input = $(this);
                if (input.attr('name') != elementName && input.val() == element.val())
                    repeated = true;
            });
            return repeated;
        },

        // constructor
        constructor: function (opts) {
            // merge options with default
            this._opts = $.extend({
                invalidDomain: 'Invalid domain.',
                duplicateDomain: 'Domain has already been assigned.'
            }, opts);
        },

        // public methods/variables

        languages: null,
        language: null,
        domains: null,

        addDomain: function () {
            this.domains.push({
                Name: "",
                Lang: ""
            });
        },
      
        init: function () {
            var self = this;
            
            self.domains = ko.observableArray(self._opts.domains);
            self.languages = self._opts.languages;
            self.language = self._opts.language;
            self.removeDomain = function() { self.domains.remove(this); };

            ko.applyBindings(self);

            $.validator.addMethod("domain", function (value, element, param) {
                var re = /^(http[s]?:\/\/)?([-\w]+(\.[-\w]+)*)(:\d+)?(\/[-\w]*)?$/gi;
                return this.optional(element) || re.test(value);
            }, self._opts.invalidDomain);

            $.validator.addMethod("duplicate", function (value, element, param) {
                return $(element).nextAll('input').val() == 0 && !self._isRepeated($(element));
            }, self._opts.duplicateDomain);
            
            $.validator.addClassRules({
                domain: { domain: true },
                duplicate: { duplicate: true }
            });

            $('form').validate({
                debug: true,
                focusCleanup: true,
                onkeyup: false
            });

            $('form input.domain').live('focus', function(event) {
                if (event.type != 'focusin') return;
                $(this).nextAll('input').val(0);
            });
            
            // force validation *now*
            $('form').valid();

            $('#btnSave').click(function () {
                if (!$('form').valid())
                    return false;
                
                var mask = $('#komask');
                var masked = mask.next();
                mask.height(masked.height());
                mask.width(masked.width());
                mask.show();
                
                var data = { nodeId: self._opts.nodeId, language: self.language ? self.language : 0, domains: self.domains };
                $.post(self._opts.restServiceLocation + 'SaveLanguageAndDomains', ko.toJSON(data), function (json) {
                    mask.hide();

                    if (json.Valid) {
                        UmbClientMgr.closeModalWindow();
                    }
                    else {
                        var inputs = $('form input.domain');
                        inputs.each(function() { $(this).nextAll('input').val(0); });
                        for (var i = 0; i < json.Domains.length; i++) {
                            var d = json.Domains[i];
                            if (d.Duplicate == 1)
                                inputs.each(function() {
                                    var input = $(this);
                                    if (input.val() == d.Name)
                                        input.nextAll('input').val(1);
                                });
                        }
                        $('form').valid();
                    }
                })
                .fail(function (xhr, textStatus, errorThrown) {
                    mask.css('opacity', 1).css('color', "#ff0000").html(xhr.responseText);
                });
                return false;
            });
        }

    });

    // set defaults for jQuery ajax calls
    $.ajaxSetup({
        dataType: 'json',
        cache: false,
        contentType: 'application/json; charset=utf-8'
    });
    
})(jQuery);
