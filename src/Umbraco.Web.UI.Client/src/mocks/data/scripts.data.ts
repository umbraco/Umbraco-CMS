import { UmbEntityData } from './entity.data.js';
import { createFileSystemTreeItem, createTextFileItem } from './utils.js';
import {
	CreateTextFileViewModelBaseModel,
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
	PartialViewResponseModel,
	PartialViewSnippetResponseModel,
	ScriptResponseModel,
	SnippetItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

type ScriptsDataItem = ScriptResponseModel & FileSystemTreeItemPresentationModel & { id: string };

export const treeData: Array<ScriptsDataItem> = [
	{
		id: 'some-folder',
		path: 'some-folder',
		isFolder: true,
		name: 'some-folder',
		type: 'script',
		hasChildren: true,
	},
	{
		id: 'another-folder',
		path: 'another-folder',
		isFolder: true,
		name: 'another-folder',
		type: 'script',
		hasChildren: true,
	},
	{
		id: 'very important folder',
		path: 'very important folder',
		isFolder: true,
		name: 'very important folder',
		type: 'script',
		hasChildren: true,
	},
	{
		id: 'some-folder/ugly script.js',
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
		id: 'some-folder/nice script.js',
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
		id: 'another-folder/only bugs.js',
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
		id: 'very important folder/no bugs at all.js',
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
		id: 'very important folder/nope.js',
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
];

class UmbScriptsTreeData extends UmbEntityData<FileSystemTreeItemPresentationModel> {
	constructor() {
		super(treeData);
	}

	getTreeRoot(): PagedFileSystemTreeItemPresentationModel {
		const items = this.data.filter((item) => item.path?.includes('/') === false);
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(parentPath: string): PagedFileSystemTreeItemPresentationModel {
		const items = this.data.filter((item) => item.path?.startsWith(parentPath + '/'));
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(paths: Array<string>): Array<FileSystemTreeItemPresentationModel> {
		const items = this.data.filter((item) => paths.includes(item.path ?? ''));
		return items.map((item) => createFileSystemTreeItem(item));
	}
}

export const umbScriptsTreeData = new UmbScriptsTreeData();

class UmbScriptsData extends UmbEntityData<ScriptResponseModel> {
	constructor() {
		super(treeData);
	}

	getPartialView(path: string): PartialViewResponseModel | undefined {
		return createTextFileItem(this.data.find((item) => item.path === path));
	}

	insertPartialView(item: CreateTextFileViewModelBaseModel) {
		const newItem: ScriptsDataItem = {
			...item,
			path: `${item.parentPath}/${item.name}.js}`,
			id: `${item.parentPath}/${item.name}.js}`,
			isFolder: false,
			hasChildren: false,
			type: 'script',
		};

		this.insert(newItem);
		return newItem;
	}
}

export const umbPartialViewsData = new UmbScriptsData();
