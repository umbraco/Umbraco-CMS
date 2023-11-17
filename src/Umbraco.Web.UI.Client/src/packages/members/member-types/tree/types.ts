import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbMemberTypeTreeItemModel = EntityTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbMemberTypeTreeRootModel = EntityTreeItemResponseModel & UmbEntityTreeRootModel;
