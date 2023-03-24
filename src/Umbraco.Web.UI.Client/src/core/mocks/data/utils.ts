import type {
	ContentTreeItemResponseModel,
	DocumentTreeItemResponseModel,
	DocumentTypeTreeItemResponseModel,
	EntityTreeItemResponseModel,
	FolderTreeItemResponseModel,
	DocumentTypeResponseModel,
	DocumentResponseModel,
	FileSystemTreeItemPresentationModel,
} from '@umbraco-cms/backoffice/backend-api';

export const createEntityTreeItem = (item: any): EntityTreeItemResponseModel => {
	return {
		$type: '',
		name: item.name,
		type: item.type,
		icon: item.icon,
		hasChildren: item.hasChildren,
		key: item.key,
		isContainer: item.isContainer,
		parentKey: item.parentKey,
	};
};

export const createFolderTreeItem = (item: any): FolderTreeItemResponseModel => {
	return {
		...createEntityTreeItem(item),
		isFolder: item.isFolder,
	};
};

// TODO: remove isTrashed type extension when we have found a solution to trashed items
export const createContentTreeItem = (item: any): ContentTreeItemResponseModel & { isTrashed: boolean } => {
	return {
		...createEntityTreeItem(item),
		noAccess: item.noAccess,
		isTrashed: item.isTrashed,
	};
};

// TODO: remove isTrashed type extension when we have found a solution to trashed items
export const createDocumentTreeItem = (
	item: DocumentResponseModel
): DocumentTreeItemResponseModel & { isTrashed: boolean } => {
	return {
		...createContentTreeItem(item),
		/*
		noAccess: item.noAccess,
		isProtected: item.isProtected,
		isPublished: item.isPublished,
		isEdited: item.isEdited,
		isTrashed: item.isTrashed,
		*/
	};
};

export const createDocumentTypeTreeItem = (item: DocumentTypeResponseModel): DocumentTypeTreeItemResponseModel => {
	return {
		...createFolderTreeItem(item),
		isElement: item.isElement,
	};
};

export const createFileSystemTreeItem = (item: any): FileSystemTreeItemPresentationModel => {
	return {
		...createFolderTreeItem(item),
		path: item.path,
	};
};
