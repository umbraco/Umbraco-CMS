import type { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentTypeTreeItemModel
	extends Omit<DocumentTypeTreeItemResponseModel, 'icon'>,
		UmbEntityTreeItemModel {}
// TODO: TREE STORE TYPE PROBLEM:
export interface UmbDocumentTypeTreeRootModel
	extends Omit<Omit<DocumentTypeTreeItemResponseModel, 'id'>, 'icon'>,
		UmbEntityTreeRootModel {}
