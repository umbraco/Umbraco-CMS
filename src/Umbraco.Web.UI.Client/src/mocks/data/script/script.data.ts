import { FileSystemTreeItemPresentationModel, ScriptResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbMockScriptModel = ScriptResponseModel & FileSystemTreeItemPresentationModel;

export const data: Array<UmbMockScriptModel> = [
	{
		path: 'some-folder',
		isFolder: true,
		name: 'some-folder',
		type: 'script',
		hasChildren: true,
		content: '',
	},
	{
		path: 'another-folder',
		isFolder: true,
		name: 'another-folder',
		type: 'script',
		hasChildren: true,
		content: '',
	},
	{
		path: 'very important folder',
		isFolder: true,
		name: 'very important folder',
		type: 'script',
		hasChildren: true,
		content: '',
	},
	{
		path: 'some-folder/ugly script.js',
		isFolder: false,
		name: 'ugly script.js',
		type: 'script',
		hasChildren: false,
		content: `function makeid(length) {
			var result           = '';
			var characters       = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
			var charactersLength = characters.length;
			for ( var i = 0; i < length; i++ ) {
			   result += characters.charAt(Math.floor(Math.random() * charactersLength));
			}
			return result;
		 }

		 console.log(makeid(5));`,
	},
	{
		path: 'some-folder/nice script.js',
		isFolder: false,
		name: 'nice script.js',
		type: 'script',
		hasChildren: false,
		content: `var items = {
			"item_1": "1",
			"item_2": "2",
			"item_3": "3"
		}
		for (var item in items) {
			console.log(items[item]);
		}`,
	},
	{
		path: 'another-folder/only bugs.js',
		isFolder: false,
		name: 'only bugs.js',
		type: 'script',
		hasChildren: false,
		content: `var my_arr = [4, '', 0, 10, 7, '', false, 10];

		my_arr = my_arr.filter(Boolean);

		console.log(my_arr);`,
	},
	{
		path: 'very important folder/no bugs at all.js',
		isFolder: false,
		name: 'no bugs at all.js',
		type: 'script',
		hasChildren: false,
		content: `const date_str = "07/20/2021";
		const date = new Date(date_str);
		const full_day_name = date.toLocaleDateString('default', { weekday: 'long' });
		// -> to get full day name e.g. Tuesday

		const short_day_name = date.toLocaleDateString('default', { weekday: 'short' });
		console.log(short_day_name);
		// -> TO get the short day name e.g. Tue`,
	},
	{
		path: 'very important folder/nope.js',
		isFolder: false,
		name: 'nope.js',
		type: 'script',
		hasChildren: false,
		content: `// Define an object
		const employee = {
			"name": "John Deo",
			"department": "IT",
			"project": "Inventory Manager"
		};

		// Remove a property
		delete employee["project"];

		console.log(employee);`,
	},
	{
		path: 'very important folder/file-with-dash.js',
		isFolder: false,
		name: 'file-with-dash.js',
		type: 'script',
		hasChildren: false,
		content: `alert('hello file with dash');`,
	},
];
