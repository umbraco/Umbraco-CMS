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
		name: 'RTE Styles',
		path: '/rte-styles.css',
		parent: null,
		isFolder: false,
		hasChildren: false,
		content: `
/** RTE Stylesheet */

#editor {
	background-color: pink;
	font-size: 1.5rem;
}

h2 {
	color: red;
	font-size: 2rem;
}

h3 {
	color: blue;
	font-size: 1.75rem;
}

h4 {
	color: green;
	font-size: 1.5rem;
}`,
	},
	{
		name: 'RTE Styles 2',
		path: '/rte-styles-2.css',
		parent: null,
		isFolder: false,
		hasChildren: false,
		content: `
/** RTE Stylesheet 2 */

body {
	font-family: cursive;
}

span {
	color: red;
}

span {
	color: blue;
}

span {
	color: green;
}`,
	},
	{
		name: 'Folder for website',
		path: '/folder-for-website',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'Website Styles',
		path: '/folder-for-website/website-styles.css',
		parent: {
			path: '/folder-for-website',
		},
		hasChildren: false,
		isFolder: false,
		content: `
/** Website Stylesheet */

body {
	background-color: #ffb7d3;
	color: #b57790;
	font-family: sans-serif;
}
`,
	},
];
