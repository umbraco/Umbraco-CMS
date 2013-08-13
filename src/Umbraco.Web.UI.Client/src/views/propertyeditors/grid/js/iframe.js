$(function(){
    var editors = $('[data-editor]');
    var p = parent.$(parent.document);


    editors.addClass("editor");

    editors.on("click", function (event) {
        event.preventDefault();

      //  parent.document.fire("umbraco.grid.click");
      	var el = this;
      	var e = jQuery.Event("umbraco.grid.click", {editor: $(el).data("editor"), element: el});
        p.trigger( e );
    });
});



