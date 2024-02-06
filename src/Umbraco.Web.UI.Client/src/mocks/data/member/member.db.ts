import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import type { UmbMockDataTypeModel as UmbMockMemberModel } from './member.data.js';
import { data } from './member.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateMemberRequestModel,
	MemberItemResponseModel,
	MemberResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

class UmbMemberMockDB extends UmbEntityMockDbBase<UmbMockMemberModel> {
	item = new UmbMockEntityItemManager<UmbMockMemberModel>(this, itemResponseMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMemberModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockMemberModel>) {
		super(data);
	}
}

const createDetailMockMapper = (request: CreateMemberRequestModel): UmbMockMemberModel => {
	return {
		email: request.email,
		failedPasswordAttempts: 0,
		groups: request.groups,
		id: request.id ? request.id : UmbId.new(),
		isApproved: request.isApproved,
		isLockedOut: false,
		isTwoFactorEnabled: false,
		lastLockoutDate: null,
		lastLoginDate: null,
		lastPasswordChangeDate: null,
		memberType: request.memberType,
		username: request.username,
		values: request.values,
		variants: request.variants,
	};
};

const detailResponseMapper = (item: UmbMockMemberModel): MemberResponseModel => {
	return {
		email: item.email,
		failedPasswordAttempts: item.failedPasswordAttempts,
		groups: item.groups,
		id: item.id,
		isApproved: item.isApproved,
		isLockedOut: item.isLockedOut,
		isTwoFactorEnabled: item.isTwoFactorEnabled,
		lastLockoutDate: item.lastLockoutDate,
		lastLoginDate: item.lastLoginDate,
		lastPasswordChangeDate: item.lastPasswordChangeDate,
		memberType: item.memberType,
		username: item.userName,
		values: item.values,
		variants: item.variants,
	};
};

const itemResponseMapper = (item: UmbMockMemberModel): MemberItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		memberType: item.memberType,
		variants: item.variants,
	};
};

export const umbDataTypeMockDb = new UmbMemberMockDB(data);
