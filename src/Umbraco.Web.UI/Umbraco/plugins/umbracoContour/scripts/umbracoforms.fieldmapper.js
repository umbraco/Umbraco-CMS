jQuery(document).ready(function () {

    var mapper = jQuery("#fieldMapper");
    var lastfield = jQuery("div.mapping:last", mapper);

    jQuery("<h5 class='addmapping'>Add mapping</h5>").insertBefore(lastfield);

    //show remove on existing mappings
    jQuery("div.mapping").not("div.mapping:last").children("a").show();

    //make sure changes are also stored
    jQuery("div.mapping").not("div.mapping:last").each(function () {
        jQuery("select, input.alias, input.value", this).change(function () {
            storeValues();
        });
    });
})


function removeValue(link){
          
          var clone = jQuery(link).parent().clone();
          jQuery(link).parent().remove();
          
          var mapper = jQuery("#fieldMapper"); 
          
          if(jQuery("div.mapping", mapper).length == 0){
           jQuery("input", clone).val("");
           jQuery("select", clone).val("");
           jQuery("a", clone).hide();
           
           clone.appendTo(mapper); 
          }
          
          storeValues();   
}

function pickfield(select) {

    var control = jQuery(select);
    
    if (control.val() == "__setValue") {
        jQuery("input.value", control.parent()).show();
        jQuery("select", control.parent()).hide();
    }

    
}

function addValue(){
        
          var mapper = jQuery("#fieldMapper"); 
          var field = jQuery("div.mapping:first", mapper);

          jQuery("a", mapper).show();
          
          var clone =  field.clone();
          jQuery("input", clone).val("");
          jQuery("select", clone).val("").show();
          jQuery("input.value", clone).hide();
          jQuery("a", clone).hide();

          jQuery(".addmapping").appendTo(mapper);
          clone.appendTo(mapper);      
    
          storeValues();   
}

function storeValues(){
        
        var mapper = jQuery("#fieldMapper"); 
        
        var hiddenField = jQuery("#fieldmapperValues"); 
        var vals = "";
        
        jQuery("div.mapping", mapper).each(function(){
            var alias = jQuery("input.alias", this).val();
            
            if(alias != ""){
            var field = jQuery("select", this).val();
            var defaultVal = jQuery("input.value", this).val();
            
            vals +=  alias + "," + field + "," + defaultVal + ";";
            }
                      
        })        
       
        
        hiddenField.val(vals);
    }