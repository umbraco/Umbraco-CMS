import { UmbData } from './data.js';
import { UmbEntityData } from './entity.data.js';
import { createFileSystemTreeItem, createTextFileItem } from './utils.js';
import {
	CreatePathFolderRequestModel,
	CreateTextFileViewModelBaseModel,
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
	ScriptResponseModel,
	UpdateScriptRequestModel,
} from '@umbraco-cms/backoffice/backend-api';

type ScriptsDataItem = ScriptResponseModel & FileSystemTreeItemPresentationModel;

export const data: Array<ScriptsDataItem> = [
	{
		path: 'some-folder',
		isFolder: true,
		name: 'some-folder',
		type: 'script',
		hasChildren: true,
	},
	{
		path: 'another-folder',
		isFolder: true,
		name: 'another-folder',
		type: 'script',
		hasChildren: true,
	},
	{
		path: 'very important folder',
		isFolder: true,
		name: 'very important folder',
		type: 'script',
		hasChildren: true,
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
];

class UmbScriptsData extends UmbData<ScriptsDataItem> {
	constructor() {
		super(data);
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

	getFolder(path: string): FileSystemTreeItemPresentationModel {
		const items = data.filter((item) => item.isFolder && item.path === path);
		return items as FileSystemTreeItemPresentationModel;
	}

	postFolder(payload: CreatePathFolderRequestModel) {
		const newFolder = {
			path: `${payload.parentPath ?? ''}/${payload.name}`,
			isFolder: true,
			name: payload.name,
			type: 'script',
			hasChildren: false,
		};
		return this.insert(newFolder);
	}

	deleteFolder(path: string) {
		return this.delete([path]);
	}

	getScript(path: string): ScriptResponseModel | undefined {
		return createTextFileItem(this.data.find((item) => item.path === path));
	}

	insertScript(item: CreateTextFileViewModelBaseModel) {
		const newItem: ScriptsDataItem = {
			...item,
			path: `${item.parentPath}/${item.name}.js}`,
			isFolder: false,
			hasChildren: false,
			type: 'script',
		};

		this.insert(newItem);
		return newItem;
	}

	insert(item: ScriptsDataItem) {
		const exits = this.data.find((i) => i.path === item.path);

		if (exits) {
			throw new Error(`Item with path ${item.path} already exists`);
		}

		this.data.push(item);

		return item;
	}

	updateData(updateItem: UpdateScriptRequestModel) {
		const itemIndex = this.data.findIndex((item) => item.path === updateItem.existingPath);
		const item = this.data[itemIndex];
		if (!item) return;

		// TODO: revisit this code, seems like something we can solve smarter/type safer now:
		const itemKeys = Object.keys(item);
		const newItem = { ...item };

		for (const [key] of Object.entries(updateItem)) {
			if (itemKeys.indexOf(key) !== -1) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				newItem[key] = updateItem[key];
			}
		}
		// Specific to fileSystem, we need to update path based on name:
		const dirName = updateItem.existingPath?.substring(0, updateItem.existingPath.lastIndexOf('/'));
		newItem.path = `${dirName}${dirName ? '/' : ''}${updateItem.name}`;

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.data[itemIndex] = newItem;
	}

	delete(paths: Array<string>) {
		const deletedPaths = this.data
			.filter((item) => {
				if (!item.path) throw new Error('Item has no path');
				paths.includes(item.path);
			})
			.map((item) => item.path);

		this.data = this.data.filter((item) => {
			if (!item.path) throw new Error('Item has no path');
			paths.indexOf(item.path) === -1;
		});

		return deletedPaths;
	}
}

export const umbScriptsData = new UmbScriptsData();
