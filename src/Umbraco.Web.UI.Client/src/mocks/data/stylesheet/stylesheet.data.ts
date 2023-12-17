import { FileSystemTreeItemPresentationModel, StylesheetResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbMockStylesheetModel = StylesheetResponseModel & FileSystemTreeItemPresentationModel & { icon?: string };

export const data: Array<UmbMockStylesheetModel> = [
	{
		path: 'Stylesheet File 1.css',
		icon: 'style',
		isFolder: false,
		name: 'Stylesheet File 1.css',
		type: 'stylesheet',
		hasChildren: false,
		content: `
		/** Stylesheet 1 */

		h1 {
	color: blue;
}

/**umb_name:bjjh*/
h1 {
	color: blue;
}

/**umb_name:comeone*/
h1 {
	color: blue;
}

/**umb_name:lol*/
h1 {
	color: blue;
}`,
	},
	{
		path: 'Stylesheet File 2.css',
		isFolder: false,
		icon: 'style',
		name: 'Stylesheet File 2.css',
		type: 'stylesheet',
		hasChildren: false,
		content: `
		/** Stylesheet 2 */
h1 {
	color: green;
}

/**umb_name:HELLO*/
h1 {
	color: green;
}

/**umb_name:SOMETHING*/
h1 {
	color: green;
}

/**umb_name:NIOCE*/
h1 {
	color: green;
}`,
	},
	{
		path: 'Folder 1',
		name: 'Folder 1',
		isFolder: true,
		icon: 'folder',
		type: 'stylesheet',
		hasChildren: true,
		content: '',
	},
	{
		path: 'Folder 1/Stylesheet File 3.css',
		name: 'Stylesheet File 3.css',
		type: 'stylesheet',
		hasChildren: false,
		isFolder: false,
		content: `h1 {
	color: pink;
}

/**umb_name:ONE*/
h1 {
	color: pink;
}

/**umb_name:TWO*/
h1 {
	color: pink;
}

/**umb_name:THREE*/
h1 {
	color: pink;
}`,
	},
];
