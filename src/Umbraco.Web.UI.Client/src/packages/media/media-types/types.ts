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

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaTypeAvailableCompositionRequestModel extends UmbContentTypeAvailableCompositionRequestModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaTypeCompositionCompatibleModel extends UmbContentTypeCompositionCompatibleModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaTypeCompositionReferenceModel extends UmbContentTypeCompositionReferenceModel {}
