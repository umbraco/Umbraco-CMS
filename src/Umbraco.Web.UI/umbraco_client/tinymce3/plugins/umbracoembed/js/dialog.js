tinyMCEPopup.requireLangPack();

var UmbracoEmbedDialog = {
    insert: function () {
        // Insert the contents from the input into the document
        tinyMCEPopup.editor.execCommand('mceInsertContent', false, $('#source').val());
        tinyMCEPopup.close();
    },
    showPreview: function () {

        $('#insert').attr('disabled', 'disabled');

        var url = $('#url').val();
        var width = $('#width').val(); ;
        var height = $('#height').val(); ;

        $('#preview').html('<img src="img/ajax-loader.gif" alt="loading"/>');
        $('#source').val('');
        $.ajax(
                    {
                        type: 'POST',
                        async: true,
                        url: '../../../../umbraco/webservices/api/mediaservice.asmx/Embed',
                        data: '{ url: "' + url + '", width: "' + width + '", height: "' + height + '" }',
                        contentType: 'application/json; charset=utf-8',
                        dataType: 'json',
                        success: function (msg) {
                            var resultAsJson = msg.d;
                            switch (resultAsJson.Status) {
                                case 0:
                                    //not supported
                                    $('#preview').html('Not Supported');
                                    break;
                                case 1:
                                    //error
                                    $('#preview').html('Error');
                                    break;
                                case 2:
                                    $('#preview').html(resultAsJson.Markup);
                                    $('#source').val(resultAsJson.Markup);
                                    if (resultAsJson.SupportsDimensions) {
                                        $('#dimensions').show();
                                    } else {
                                        $('#dimensions').hide();
                                    }
                                    $('#insert').removeAttr('disabled');
                                    break;
                            }

                        },
                        error: function (xhr, ajaxOptions, thrownError) {
                            $('#preview').html("Error");
                            //alert(xhr.status);
                            //alert(thrownError);   
                        }
                    });
    },
    beforeResize: function () {
        this.width = parseInt($('#width').val(), 10);
        this.height = parseInt($('#height').val(), 10);
    },
    changeSize: function (type) {
        var width, height, scale, size;

        if ($('#constrain').is(':checked')) {
            width = parseInt($('#width').val(), 10);
            height = parseInt($('#height').val(), 10);
            if (type == 'width') {
                this.height = Math.round((width / this.width) * height);
                $('#height').val(this.height);
            } else {
                this.width = Math.round((height / this.height) * width);
                $('#width').val(this.width);
            }
        }

        if ($('#url').val() != '') {
            UmbracoEmbedDialog.showPreview();
        }
    }
};

