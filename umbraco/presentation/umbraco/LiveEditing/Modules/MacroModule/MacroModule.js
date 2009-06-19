/********************* Live Editing MacroModule functions *********************/
    
function MacroOnDrop( sender, e )
{
var container = e.get_container();
var item = e.get_droppedItem();
var position = e.get_position();

//alert( String.format( "Container: {0}, Item: {1}, Position: {2}", container.id, item.id, position ) );

var instanceId = parseInt(item.getAttribute("InstanceId"));
var columnNo = parseInt(container.getAttribute("columnNo"));
var row = position;

}

function okAddMacro(sender, e)
{
    $find('ModalMacro').hide();
    /*__doPostBack('AddMacro', e); */
}
