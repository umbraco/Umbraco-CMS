import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbTemplateTreeItemModel extends EntityTreeItemResponseModel, UmbEntityTreeItemModel {}
// TODO: TREE STORE TYPE PROBLEM:
export interface UmbTemplateTreeRootModel extends Omit<EntityTreeItemResponseModel, 'id'>, UmbEntityTreeRootModel {}
