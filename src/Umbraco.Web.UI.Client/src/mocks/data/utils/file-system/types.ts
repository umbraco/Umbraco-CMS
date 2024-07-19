import type { FileSystemFolderModel } from '@umbraco-cms/backoffice/external/backend-api';

// Temp mock model until they are moved to msw
export type CreateFileRequestModel = {
	name: string;
	parent?: FileSystemFolderModel | null;
	content: string;
};

export type FileResponseModel = {
	path: string;
	name: string;
	parent?: FileSystemFolderModel | null;
	content: string;
};

export type UpdateFileRequestModel = {
	content: string;
};

export type FileItemResponseModel = {
	path: string;
	name: string;
	parent?: FileSystemFolderModel | null;
	isFolder: boolean;
};
