import type {
	ContentTreeItemResponseModel,
	DocumentTreeItemResponseModel,
	DocumentTypeTreeItemResponseModel,
	EntityTreeItemResponseModel,
	FolderTreeItemResponseModel,
	DocumentTypeResponseModel,
	FileSystemTreeItemPresentationModel,
	DocumentResponseModel,
	TextFileResponseModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';

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

// TODO: remove isTrashed type extension when we have found a solution to trashed items
export const createContentTreeItem = (item: any): ContentTreeItemResponseModel & { isTrashed: boolean } => {
	// TODO: There we have to adapt to variants as part of the tree model:
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

export const createDocumentTypeTreeItem = (item: DocumentTypeResponseModel): DocumentTypeTreeItemResponseModel => {
	return {
		...createEntityTreeItem(item),
		type: 'document-type',
		isElement: item.isElement,
	};
};

export const createFileSystemTreeItem = (item: any): FileSystemTreeItemPresentationModel => {
	return {
		name: item.name,
		type: item.type,
		hasChildren: item.hasChildren,
		path: item.path,
		isFolder: item.isFolder,
	};
};

export const createTextFileItem = (item: any): TextFileResponseModelBaseModel => ({
	path: item.path,
	name: item.name,
	content: item.content,
});

export const createItem = (item: any): any => ({
	path: item.path,
	name: item.name,
	icon: item.icon,
});
