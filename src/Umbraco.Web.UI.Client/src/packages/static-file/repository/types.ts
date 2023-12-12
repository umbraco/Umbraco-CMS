import { StaticFileItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbStaticFileTreeItemModel extends Omit<StaticFileItemResponseModel, 'icon'>, UmbEntityTreeItemModel {}
export interface UmbStaticFileTreeRootModel
	extends Omit<Omit<StaticFileItemResponseModel, 'id'>, 'icon'>,
		UmbEntityTreeRootModel {}
