import type {
	FileSystemTreeItemPresentationModel,
	StylesheetItemResponseModel,
	StylesheetResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockStylesheetModel = StylesheetResponseModel &
	FileSystemTreeItemPresentationModel &
	StylesheetItemResponseModel;

export const data: Array<UmbMockStylesheetModel> = [
	{
		name: 'Stylesheet File 1.css',
		path: '/Stylesheet File 1.css',
		parent: null,
		isFolder: false,
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
		name: 'Stylesheet File 2.css',
		path: '/Stylesheet File 2.css',
		parent: null,
		isFolder: false,
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
		name: 'Folder 1',
		path: '/Folder 1',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'Stylesheet File 3.css',
		path: '/Folder 1/Stylesheet File 3.css',
		parent: {
			path: '/Folder 1',
		},
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
