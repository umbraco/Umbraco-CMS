import { dataSet } from '../sets/index.js';
import type { UserGroupItemResponseModel, UserGroupResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockUserGroupModel = UserGroupResponseModel & UserGroupItemResponseModel;

export const data: Array<UmbMockUserGroupModel> = dataSet.userGroup;
