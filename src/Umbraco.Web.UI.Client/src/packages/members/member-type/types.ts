import type { UmbMemberTypeEntityType } from './entity.js';
import type {
	UmbContentTypeAvailableCompositionRequestModel,
	UmbContentTypeCompositionCompatibleModel,
	UmbContentTypeCompositionReferenceModel,
	UmbContentTypeModel,
} from '@umbraco-cms/backoffice/content-type';

export type * from './entity.js';
export type * from './tree/types.js';

export interface UmbMemberTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbMemberTypeEntityType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberTypeAvailableCompositionRequestModel extends UmbContentTypeAvailableCompositionRequestModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberTypeCompositionCompatibleModel extends UmbContentTypeCompositionCompatibleModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberTypeCompositionReferenceModel extends UmbContentTypeCompositionReferenceModel {}
