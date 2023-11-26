import type { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbDocumentTypeTreeItemModel = DocumentTypeTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbDocumentTypeTreeRootModel = DocumentTypeTreeItemResponseModel & UmbEntityTreeRootModel;
