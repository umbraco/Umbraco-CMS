import type { UmbMemberTypeEntityType } from './entity.js';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';

export interface UmbMemberTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbMemberTypeEntityType;
}
