import { dataSet } from './sets/index.js';
import type { MemberResponseModel, MemberItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMemberModel = MemberResponseModel & MemberItemResponseModel;

export const data: Array<UmbMockMemberModel> = dataSet.member;
