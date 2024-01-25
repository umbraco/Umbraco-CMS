import type {
	ContentTreeItemResponseModel,
	FolderTreeItemResponseModel,
	FileSystemTreeItemPresentationModel,
	FileSystemItemResponseModelBaseModel,
	MediaTreeItemResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbMediaDetailModel } from '@umbraco-cms/backoffice/media';

export const createEntityTreeItem = (item: any): NamedEntityTreeItemResponseModel => {
	return {
		name: item.name,
		type: item.type,
		hasChildren: item.hasChildren,
		id: item.id,
		parent: item.parent,
	};
};

export const folderTreeItemMapper = (item: any): FolderTreeItemResponseModel => {
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

export const createMediaTreeItem = (item: UmbMediaDetailModel): MediaTreeItemResponseModel => {
	return {
		...createContentTreeItem(item),
		type: 'media',
		mediaType: {
			// TODO: get this from media type
			id: '',
			icon: '',
			hasListView: false,
		},
	};
};

export const createFileSystemTreeItem = (item: any): Omit<FileSystemTreeItemPresentationModel, 'type'> => {
	return {
		path: item.path,
		parent: item.parent ?? null,
		name: item.name,
		hasChildren: item.hasChildren ?? false,
		isFolder: item.isFolder ?? false,
	};
};

export const createFileItemResponseModelBaseModel = (item: any): FileSystemItemResponseModelBaseModel => ({
	path: item.path,
	name: item.name,
	parent: item.parent,
	isFolder: item.isFolder,
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
