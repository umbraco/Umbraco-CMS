import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbMemberTreeItemModel = EntityTreeItemResponseModel & UmbEntityTreeItemModel;
export type UmbMemberTreeRootModel = EntityTreeItemResponseModel & UmbEntityTreeRootModel;
