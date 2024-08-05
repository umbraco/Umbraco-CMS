import type { UmbMediaTypeEntityType } from './entity.js';
import type {
	UmbContentTypeAvailableCompositionRequestModel,
	UmbContentTypeCompositionCompatibleModel,
	UmbContentTypeCompositionReferenceModel,
	UmbContentTypeModel,
} from '@umbraco-cms/backoffice/content-type';

export interface UmbMediaTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbMediaTypeEntityType;
}

export interface UmbMediaTypeAvailableCompositionRequestModel extends UmbContentTypeAvailableCompositionRequestModel {}

export interface UmbMediaTypeCompositionCompatibleModel extends UmbContentTypeCompositionCompatibleModel {}

export interface UmbMediaTypeCompositionReferenceModel extends UmbContentTypeCompositionReferenceModel {}
