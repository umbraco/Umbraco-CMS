import { RecycleBinItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentRecycleBinTreeItemModel
	extends Omit<RecycleBinItemResponseModel, 'icon'>,
		UmbEntityTreeItemModel {}
// TODO: TREE STORE TYPE PROBLEM:
export interface UmbDocumentRecycleBinTreeRootModel
	extends Omit<Omit<RecycleBinItemResponseModel, 'id'>, 'icon'>,
		UmbEntityTreeRootModel {}
