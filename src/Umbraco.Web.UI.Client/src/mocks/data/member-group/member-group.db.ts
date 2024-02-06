import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import type { UmbMockMemberGroupModel } from './member-group.data.js';
import { data } from './member-group.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

class UmbMemberGroupMockDB extends UmbEntityMockDbBase<UmbMockMemberGroupModel> {
	item = new UmbMockEntityItemManager<UmbMockMemberGroupModel>(this, itemResponseMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMemberGroupModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockMemberGroupModel>) {
		super(data);
	}
}

const createDetailMockMapper = (request: any): UmbMockMemberGroupModel => {
	return {
		id: request.id ? request.id : UmbId.new(),
		name: request.name,
	};
};

const detailResponseMapper = (item: UmbMockMemberGroupModel): any => {
	return {
		id: item.id,
		name: item.name,
	};
};

const itemResponseMapper = (item: UmbMockMemberGroupModel): MemberGroupItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
	};
};

export const umbMemberGroupMockDb = new UmbMemberGroupMockDB(data);
