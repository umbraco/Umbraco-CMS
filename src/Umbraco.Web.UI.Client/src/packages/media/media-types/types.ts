import type { UmbMediaTypeEntityType } from './entity.js';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';

export interface UmbMediaTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbMediaTypeEntityType;
}
