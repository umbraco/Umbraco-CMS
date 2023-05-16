import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: temp until we have a proper stylesheet model
export interface PartialViewDetails extends FileSystemTreeItemPresentationModel {
	content: string;
}

export const PARTIAL_VIEW_ENTITY_TYPE = 'partial-view';
export const PARTIAL_VIEW_FOLDER_ENTITY_TYPE = 'partial-view';

export const PARTIAL_VIEW_REPOSITORY_ALIAS = 'Umb.Repository.PartialViews';

export const PARTIAL_VIEW_TREE_ALIAS = 'Umb.Tree.PartialViews';

export const UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN_ALIAS = 'Umb.Store.PartialViews.Tree';
export const UMB_PARTIAL_VIEW_STORE_CONTEXT_TOKEN_ALIAS = 'Umb.Store.PartialViews';
