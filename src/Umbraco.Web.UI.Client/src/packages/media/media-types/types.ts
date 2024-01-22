import { UmbMediaTypeEntityType } from './entity.js';
import { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';

export interface UmbMediaTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbMediaTypeEntityType;
}
