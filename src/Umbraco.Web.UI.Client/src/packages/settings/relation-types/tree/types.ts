import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbRelationTypeTreeItemModel = EntityTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbRelationTypeTreeRootModel = EntityTreeItemResponseModel & UmbEntityTreeRootModel;
