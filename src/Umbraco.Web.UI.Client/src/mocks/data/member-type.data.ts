import { dataSet } from './sets/index.js';
import type {
	MemberTypeItemResponseModel,
	MemberTypeResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMemberTypeModel = MemberTypeResponseModel &
	MemberTypeItemResponseModel & {
		hasChildren: boolean;
		parent: { id: string } | null;
		hasListView: boolean;
	};

export const data: Array<UmbMockMemberTypeModel> = dataSet.memberType;
