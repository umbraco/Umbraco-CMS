import type { UmbMockStaticFileModel } from '../../mock-data-set.types.js';

export const data: Array<UmbMockStaticFileModel> = [
	{
		path: '/some-file.js',
		parent: null,
		name: 'some-file.js',
		isFolder: false,
	},
	{
		path: '/another-file.js',
		parent: null,
		name: 'another-file.js',
		isFolder: false,
	},
	{
		path: '/Folder 1',
		parent: null,
		name: 'Folder 1',
		isFolder: true,
	},
	{
		path: '/Folder 1/File in Folder 1.js',
		parent: {
			path: '/Folder 1',
		},
		name: 'File in Folder 1.js',
		isFolder: false,
	},
];
