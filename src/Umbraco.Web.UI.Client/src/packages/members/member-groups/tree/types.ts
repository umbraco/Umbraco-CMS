import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbMemberGroupTreeItemModel = EntityTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbMemberGroupTreeRootModel = EntityTreeItemResponseModel & UmbEntityTreeRootModel;
