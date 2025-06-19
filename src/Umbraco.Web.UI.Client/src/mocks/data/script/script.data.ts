import type {
	FileSystemTreeItemPresentationModel,
	ScriptItemResponseModel,
	ScriptResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockScriptModel = ScriptResponseModel & FileSystemTreeItemPresentationModel & ScriptItemResponseModel;

export const data: Array<UmbMockScriptModel> = [
	{
		name: 'some-folder',
		path: '/some-folder',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'another-folder',
		path: '/another-folder',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'very important folder',
		path: '/very important folder',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'ugly script.js',
		path: '/some-folder/ugly script.js',
		parent: {
			path: '/some-folder',
		},
		isFolder: false,
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
		name: 'nice script.js',
		path: '/some-folder/nice script.js',
		parent: {
			path: '/some-folder',
		},
		isFolder: false,
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
		name: 'only bugs.js',
		path: '/another-folder/only bugs.js',
		parent: {
			path: '/another-folder',
		},
		isFolder: false,
		hasChildren: false,
		content: `var my_arr = [4, '', 0, 10, 7, '', false, 10];

		my_arr = my_arr.filter(Boolean);

		console.log(my_arr);`,
	},
	{
		name: 'no bugs at all.js',
		path: '/very important folder/no bugs at all.js',
		parent: {
			path: '/very important folder',
		},
		isFolder: false,
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
		name: 'nope.js',
		path: '/very important folder/nope.js',
		parent: {
			path: '/very important folder',
		},
		isFolder: false,
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
		name: 'file-with-dash.js',
		path: '/very important folder/file-with-dash.js',
		parent: {
			path: '/very important folder',
		},
		isFolder: false,
		hasChildren: false,
		content: `alert('hello file with dash');`,
	},
];
