import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityFolderManager } from '../utils/entity/entity-folder.manager.js';
import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { folderTreeItemMapper } from '../utils.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import type { UmbMockMemberTypeModel } from './member-type.data.js';
import { data } from './member-type.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateMemberTypeRequestModel,
	CreateFolderRequestModel,
	MemberTypeItemResponseModel,
	MemberTypeResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

class UmbMemberTypeMockDB extends UmbEntityMockDbBase<UmbMockMemberTypeModel> {
	tree = new UmbMockEntityTreeManager<UmbMockMemberTypeModel>(this, folderTreeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockMemberTypeModel>(this, itemResponseMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMemberTypeModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockMemberTypeModel>) {
		super(data);
	}
}

const createDetailMockMapper = (request: CreateMemberTypeRequestModel): UmbMockMemberTypeModel => {
	return {
		id: request.id ? request.id : UmbId.new(),
		parent: request.parent,
		name: request.name,
		editorAlias: request.editorAlias,
		editorUiAlias: request.editorUiAlias,
		values: request.values,
		isFolder: false,
		hasChildren: false,
	};
};

const detailResponseMapper = (item: UmbMockMemberTypeModel): MemberTypeResponseModel => {
	return {
		id: item.id,
		parent: item.parent,
		name: item.name,
		editorAlias: item.editorAlias,
		editorUiAlias: item.editorUiAlias,
		values: item.values,
	};
};

const itemResponseMapper = (item: UmbMockMemberTypeModel): MemberTypeItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
	};
};

export const umbMemberTypeMockDb = new UmbMemberTypeMockDB(data);
