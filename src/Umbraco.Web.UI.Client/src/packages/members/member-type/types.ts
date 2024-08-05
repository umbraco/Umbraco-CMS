import type { UmbMemberTypeEntityType } from './entity.js';
import type {
	UmbContentTypeAvailableCompositionRequestModel,
	UmbContentTypeCompositionCompatibleModel,
	UmbContentTypeCompositionReferenceModel,
	UmbContentTypeModel,
} from '@umbraco-cms/backoffice/content-type';

export interface UmbMemberTypeDetailModel extends UmbContentTypeModel {
	entityType: UmbMemberTypeEntityType;
}

export interface UmbMemberTypeAvailableCompositionRequestModel extends UmbContentTypeAvailableCompositionRequestModel {}

export interface UmbMemberTypeCompositionCompatibleModel extends UmbContentTypeCompositionCompatibleModel {}

export interface UmbMemberTypeCompositionReferenceModel extends UmbContentTypeCompositionReferenceModel {}
