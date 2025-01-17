import type {
	FileSystemTreeItemPresentationModel,
	StaticFileItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockStaticFileModel = StaticFileItemResponseModel & FileSystemTreeItemPresentationModel;

export const data: Array<UmbMockStaticFileModel> = [
	{
		path: '/some-file.js',
		parent: null,
		name: 'some-file.js',
		hasChildren: false,
		isFolder: false,
	},
	{
		path: '/another-file.js',
		parent: null,
		name: 'another-file.js',
		hasChildren: false,
		isFolder: false,
	},
	{
		path: '/Folder 1',
		parent: null,
		name: 'Folder 1',
		hasChildren: true,
		isFolder: true,
	},
	{
		path: '/Folder 1/File in Folder 1.js',
		parent: {
			path: '/Folder 1',
		},
		name: 'File in Folder 1.js',
		hasChildren: false,
		isFolder: false,
	},
];
