import {
	SaveUserGroupRequestModel,
	UpdateUserGroupRequestModel,
	UserGroupBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';

export interface UserGroupEntity extends UmbEntityBase {
	type: 'user-group';
	icon?: string;
}

export interface UserGroupDetails extends UserGroupEntity {
	sections: Array<string>;
	contentStartNode?: string;
	mediaStartNode?: string;
	permissions: Array<string>;
}

export interface UmbUserGroupCollectionFilterModel {
	skip?: number;
	take?: number;
}

export type UmbUserGroupDetailDataSource = UmbDataSource<
	SaveUserGroupRequestModel,
	void,
	UpdateUserGroupRequestModel,
	UserGroupBaseModel
>;
