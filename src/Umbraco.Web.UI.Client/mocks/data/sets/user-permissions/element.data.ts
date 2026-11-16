import type { UmbMockElementModel } from '../../mock-data-set.types.js';

const createFolder = (id: string, name: string): UmbMockElementModel => ({
	ancestors: [],
	id,
	createDate: '2024-01-14T09:00:00.000Z',
	parent: null,
	documentType: null,
	hasChildren: false,
	isTrashed: false,
	isFolder: true,
	name,
	variants: [],
	values: [],
	flags: [],
	noAccess: false,
});

export const data: Array<UmbMockElementModel> = [
	createFolder('permissions-folder-read-only-id', 'Folder read only'),
	createFolder('permissions-both-read-id', 'Folder both read'),
	createFolder('permissions-neither-read-id', 'Folder neither read'),
	createFolder('permissions-folder-with-element-read-id', 'Folder with element read'),
	createFolder('permissions-folder-without-element-verbs-id', 'Folder without element verbs'),
];
