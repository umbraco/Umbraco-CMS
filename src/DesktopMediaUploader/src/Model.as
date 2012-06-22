package
{
	public class Model
	{
		[Bindable] public static var url:String;
		[Bindable] public static var username:String;
		[Bindable] public static var password:String;
		[Bindable] public static var ticket:String;
		
		[Bindable] public static var displayName:String;
		[Bindable] public static var umbracoPath:String;
		[Bindable] public static var maxRequestLength:Number;
		
		[Bindable] public static var currentFolder:XML;
		
		[Bindable] public static var folderId:Number = 0;
		[Bindable] public static var folderName:String = "";
		[Bindable] public static var folderPath:String = "";
		[Bindable] public static var replaceExisting:Boolean = false;
	}
}