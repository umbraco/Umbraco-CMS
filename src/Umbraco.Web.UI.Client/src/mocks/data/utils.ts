import type {
	ContentTreeItemResponseModel,
	DocumentTreeItemResponseModel,
	EntityTreeItemResponseModel,
	FolderTreeItemResponseModel,
	FileSystemTreeItemPresentationModel,
	DocumentResponseModel,
	TextFileResponseModelBaseModel,
	FileItemResponseModelBaseModel,
	MediaTypeResponseModel,
	MediaTypeTreeItemResponseModel,
	MediaTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbMediaDetailModel } from '@umbraco-cms/backoffice/media';

export const createEntityTreeItem = (item: any): EntityTreeItemResponseModel => {
	return {
		name: item.name,
		type: item.type,
		hasChildren: item.hasChildren,
		id: item.id,
		isContainer: item.isContainer,
		parentId: item.parentId ?? null,
	};
};

export const createFolderTreeItem = (item: any): FolderTreeItemResponseModel => {
	return {
		...createEntityTreeItem(item),
		isFolder: item.isFolder,
	};
};

export const createContentTreeItem = (item: any): ContentTreeItemResponseModel => {
	// TODO: There we have to adapt to variants as part of the tree model:
	return {
		...createEntityTreeItem(item),
		noAccess: item.noAccess,
		isTrashed: item.isTrashed,
	};
};

export const createDocumentTreeItem = (item: DocumentResponseModel): DocumentTreeItemResponseModel => {
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	return {
		...createContentTreeItem(item),
		type: 'document',
		icon: 'document', // TODO: Should get this from document type...
		name: item.variants?.[0].name ?? '',
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
		hasChildren: false,
		isContainer: false,
	};
};

export const createMediaTreeItem = (item: UmbMediaDetailModel): MediaTreeItemResponseModel => {
	return {
		...createContentTreeItem(item),
		type: 'media',
		icon: 'media', // TODO: Should get this from media type...
	};
};

export const createMediaTypeTreeItem = (item: MediaTypeResponseModel): MediaTypeTreeItemResponseModel => {
	return {
		...createEntityTreeItem(item),
		type: 'media-type',
		isFolder: false,
		icon: item.icon,
	};
};

export const createFileSystemTreeItem = (item: any): FileSystemTreeItemPresentationModel => {
	return {
		name: item.name,
		type: item.type,
		hasChildren: item.hasChildren ?? false,
		path: item.path,
		parent: item.parent ?? null,
		isFolder: item.isFolder ?? false,
	};
};

export const textFileItemMapper = (item: any): TextFileResponseModelBaseModel => ({
	path: item.path,
	name: item.name,
	content: item.content,
});

export const createFileItemResponseModelBaseModel = (item: any): FileItemResponseModelBaseModel => ({
	path: item.path,
	name: item.name,
	icon: item.icon,
});

export const arrayFilter = (filterBy: Array<string>, value?: Array<string>): boolean => {
	// if a filter is not set, return all items
	if (!filterBy) {
		return true;
	}

	return filterBy.some((filterValue: string) => value?.includes(filterValue));
};

export const stringFilter = (filterBy: Array<string>, value?: string): boolean => {
	// if a filter is not set, return all items
	if (!filterBy || !value) {
		return true;
	}
	return filterBy.includes(value);
};

export const queryFilter = (filterBy: string, value?: string) => {
	if (!filterBy || !value) {
		return true;
	}

	const query = filterBy.toLowerCase();
	return value.toLowerCase().includes(query);
};
