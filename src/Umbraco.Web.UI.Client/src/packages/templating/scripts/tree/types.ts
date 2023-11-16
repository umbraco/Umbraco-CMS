import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbScriptTreeItemModel = EntityTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbScriptTreeRootModel = EntityTreeItemResponseModel & UmbEntityTreeRootModel;
