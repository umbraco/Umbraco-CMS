import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMediaTreeItemModel extends EntityTreeItemResponseModel, UmbEntityTreeItemModel {}
// TODO: TREE STORE TYPE PROBLEM:
export interface UmbMediaTreeRootModel extends Omit<EntityTreeItemResponseModel, 'id'>, UmbEntityTreeRootModel {}
