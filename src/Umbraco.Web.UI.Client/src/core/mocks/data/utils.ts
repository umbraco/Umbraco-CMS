import type {
	ContentTreeItemModel,
	DocumentTreeItemModel,
	DocumentTypeTreeItemModel,
	EntityTreeItemModel,
	FolderTreeItemModel,
} from '@umbraco-cms/backend-api';
import type { DocumentDetails, DocumentTypeDetails } from '@umbraco-cms/models';

export const createEntityTreeItem = (item: any): EntityTreeItemModel => {
	return {
		name: item.name,
		type: item.type,
		icon: item.icon,
		hasChildren: item.hasChildren,
		key: item.key,
		isContainer: item.isContainer,
		parentKey: item.parentKey,
	};
};

export const createFolderTreeItem = (item: any): FolderTreeItemModel => {
	return {
		...createEntityTreeItem(item),
		isFolder: item.isFolder,
	};
};

// TODO: remove isTrashed type extension when we have found a solution to trashed items
export const createContentTreeItem = (item: any): ContentTreeItemModel & { isTrashed: boolean } => {
	return {
		...createEntityTreeItem(item),
		noAccess: item.noAccess,
		isTrashed: item.isTrashed,
	};
};

// TODO: remove isTrashed type extension when we have found a solution to trashed items
export const createDocumentTreeItem = (item: DocumentDetails): DocumentTreeItemModel & { isTrashed: boolean } => {
	return {
		...createContentTreeItem(item),
		noAccess: item.noAccess,
		isProtected: item.isProtected,
		isPublished: item.isPublished,
		isEdited: item.isEdited,
		isTrashed: item.isTrashed,
	};
};

export const createDocumentTypeTreeItem = (item: DocumentTypeDetails): DocumentTypeTreeItemModel => {
	return {
		...createFolderTreeItem(item),
		isElement: item.isElement,
	};
};
