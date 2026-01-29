import { objectArrayFilter, queryFilter } from '../utils.js';
import type { UmbMockMemberModel } from '../data/sets/index.js';
import { dataSet } from '../data/sets/index.js';
import { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import { UmbMockEntityItemManager } from './utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import { umbMemberTypeMockDb } from './member-type.db.js';
import { UmbMockContentCollectionManager } from './utils/content/content-collection.manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import {
	MemberKindModel,
	type CreateMemberRequestModel,
	type MemberItemResponseModel,
	type MemberResponseModel,
	type MemberValueResponseModel,
	type PagedMemberResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

interface MemberFilterOptions {
	skip: number;
	take: number;
	orderBy: string;
	orderDirection: string;
	memberGroupIds: Array<{ id: string }>;
	memberTypeId: string;
	filter: string;
}

const memberGroupFilter = (filterOptions: MemberFilterOptions, item: UmbMockMemberModel) =>
	objectArrayFilter(filterOptions.memberGroupIds, item.groups, 'id');
const memberTypeFilter = (filterOptions: MemberFilterOptions, item: UmbMockMemberModel) =>
	queryFilter(filterOptions.memberTypeId, item.memberType.id);
const memberQueryFilter = (filterOptions: MemberFilterOptions, item: UmbMockMemberModel) =>
	queryFilter(filterOptions.filter, item.username);

class UmbMemberMockDB extends UmbEntityMockDbBase<UmbMockMemberModel> {
	item = new UmbMockEntityItemManager<UmbMockMemberModel>(this, itemResponseMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMemberModel>(this, createDetailMockMapper, detailResponseMapper);
	collection = new UmbMockContentCollectionManager(this, collectionItemResponseMapper);

	constructor(data: Array<UmbMockMemberModel>) {
		super(data);
	}

	filter(options: MemberFilterOptions): PagedMemberResponseModel {
		const allItems = this.getAll();

		const filterOptions: MemberFilterOptions = {
			skip: options.skip || 0,
			take: options.take || 25,
			orderBy: options.orderBy || 'name',
			orderDirection: options.orderDirection || 'asc',
			memberGroupIds: options.memberGroupIds,
			memberTypeId: options.memberTypeId || '',
			filter: options.filter,
		};

		const filteredItems = allItems.filter(
			(item) =>
				memberGroupFilter(filterOptions, item) &&
				memberTypeFilter(filterOptions, item) &&
				memberQueryFilter(filterOptions, item),
		);
		const totalItems = filteredItems.length;

		const paginatedItems = filteredItems.slice(filterOptions.skip, filterOptions.skip + filterOptions.take);

		return { total: totalItems, items: paginatedItems };
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
		kind: MemberKindModel.DEFAULT,
		memberType: {
			id: memberType.id,
			icon: memberType.icon,
		},
		username: request.username,
		values: request.values as MemberValueResponseModel[],
		flags: [],
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
		kind: item.kind,
		lastLockoutDate: item.lastLockoutDate,
		lastLoginDate: item.lastLoginDate,
		lastPasswordChangeDate: item.lastPasswordChangeDate,
		memberType: item.memberType,
		username: item.username,
		values: item.values,
		variants: item.variants,
		flags: item.flags,
	};
};

const itemResponseMapper = (item: UmbMockMemberModel): MemberItemResponseModel => {
	return {
		id: item.id,
		kind: item.kind,
		memberType: item.memberType,
		variants: item.variants,
		flags: item.flags,
	};
};

const collectionItemResponseMapper = (item: UmbMockMemberModel): any => {
	return {
		id: item.id,
		email: item.email,
		variants: item.variants,
	};
};

export const umbMemberMockDb = new UmbMemberMockDB(dataSet.member ?? []);
