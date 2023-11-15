import { type DataTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbDataTypeTreeItemModel = DataTypeTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbDataTypeTreeRootModel = DataTypeTreeItemResponseModel & UmbEntityTreeRootModel;
