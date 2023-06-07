import type {
	ContentTreeItemResponseModel,
	DocumentTreeItemResponseModel,
	DocumentTypeTreeItemResponseModel,
	EntityTreeItemResponseModel,
	FolderTreeItemResponseModel,
	DocumentTypeResponseModel,
	FileSystemTreeItemPresentationModel,
	DocumentResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const createEntityTreeItem = (item: any): EntityTreeItemResponseModel => {
	return {
		$type: '',
		name: item.name,
		type: item.type,
		icon: item.icon,
		hasChildren: item.hasChildren,
		id: item.id,
		isContainer: item.isContainer,
		parentId: item.parentId,
	};
};

export const createFolderTreeItem = (item: any): FolderTreeItemResponseModel => {
	return {
		...createEntityTreeItem(item),
		$type: 'FolderTreeItemResponseModel',
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
		...createEntityTreeItem(item),
		type: "document-type",
		isElement: item.isElement,
	};
};

export const createFileSystemTreeItem = (item: any): FileSystemTreeItemPresentationModel => {
	return {
		name: item.name,
		type: item.type,
		icon: item.icon,
		hasChildren: item.hasChildren,
		path: item.path,
		isFolder: item.isFolder,
	};
};
