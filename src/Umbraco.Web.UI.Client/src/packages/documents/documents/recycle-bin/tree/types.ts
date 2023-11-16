import { RecycleBinItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbDocumentRecycleBinTreeItemModel = RecycleBinItemResponseModel & UmbEntityTreeItemModel;
export type UmbDocumentRecycleBinTreeRootModel = RecycleBinItemResponseModel & UmbEntityTreeRootModel;
