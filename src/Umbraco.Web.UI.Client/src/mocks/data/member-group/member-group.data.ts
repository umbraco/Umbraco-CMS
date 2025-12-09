import { dataSet } from '../sets/index.js';
import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMemberGroupModel = MemberGroupItemResponseModel;

export const data: Array<UmbMockMemberGroupModel> = dataSet.memberGroup;
