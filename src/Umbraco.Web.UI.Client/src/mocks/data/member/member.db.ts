import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { umbMemberTypeMockDb } from '../member-type/member-type.db.js';
import { pagedResult } from '../utils/paged-result.js';
import { queryFilter } from '../utils.js';
import type { UmbMockMemberModel } from './member.data.js';
import { data } from './member.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateMemberRequestModel,
	MemberItemResponseModel,
	MemberResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

const memberQueryFilter = (filterOptions: any, item: UmbMockMemberModel) =>
	queryFilter(filterOptions.filter, item.name);

class UmbMemberMockDB extends UmbEntityMockDbBase<UmbMockMemberModel> {
	item = new UmbMockEntityItemManager<UmbMockMemberModel>(this, itemResponseMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMemberModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockMemberModel>) {
		super(data);
	}

	// TODO: make collection manager we can user across content types
	getCollection(options: any): any {
		const allItems = this.getAll();

		const filterOptions = {
			skip: options.skip || 0,
			take: options.take || 25,
			filter: options.filter,
		};

		const filteredItems = allItems.filter((item) => memberQueryFilter(filterOptions, item));
		const paginatedResult = pagedResult(filteredItems, filterOptions.skip, filterOptions.take);

		// return the correct properties based on a dataType
		const mappedItems = paginatedResult.items.map((item) => {
			return {
				id: item.id,
				name: item.name,
				email: item.email,
			};
		});

		return { items: mappedItems, total: paginatedResult.total };
	}
}

const createDetailMockMapper = (request: CreateMemberRequestModel): UmbMockMemberModel => {
	const memberType = umbMemberTypeMockDb.read(request.memberType.id);
	if (!memberType) throw new Error(`Member type with id ${request.memberType.id} not found`);

	const now = new Date().toString();

	return {
		email: request.email,
		failedPasswordAttempts: 0,
		groups: request.groups ? request.groups : [],
		id: request.id ? request.id : UmbId.new(),
		isApproved: request.isApproved,
		isLockedOut: false,
		isTwoFactorEnabled: false,
		lastLockoutDate: null,
		lastLoginDate: null,
		lastPasswordChangeDate: null,
		memberType: {
			id: memberType.id,
			icon: memberType.icon,
			hasListView: memberType.hasListView,
		},
		username: request.username,
		values: request.values,
		variants: request.variants.map((variantRequest) => {
			return {
				culture: variantRequest.culture,
				segment: variantRequest.segment,
				name: variantRequest.name,
				createDate: now,
				updateDate: now,
			};
		}),
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
		username: item.username,
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

export const umbMemberMockDb = new UmbMemberMockDB(data);
