import type { MediaTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMediaTypeTreeItemModel
	extends Omit<MediaTypeTreeItemResponseModel, 'icon'>,
		UmbEntityTreeItemModel {}
// TODO: TREE STORE TYPE PROBLEM:
export interface UmbMediaTypeTreeRootModel
	extends Omit<Omit<MediaTypeTreeItemResponseModel, 'id'>, 'icon'>,
		UmbEntityTreeRootModel {}
