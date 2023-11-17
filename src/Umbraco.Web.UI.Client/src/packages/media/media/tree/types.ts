import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbMediaTreeItemModel = EntityTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbMediaTreeRootModel = EntityTreeItemResponseModel & UmbEntityTreeRootModel;
