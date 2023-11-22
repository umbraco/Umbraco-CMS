import type { MediaTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbMediaTypeTreeItemModel = MediaTypeTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbMediaTypeTreeRootModel = MediaTypeTreeItemResponseModel & UmbEntityTreeRootModel;
