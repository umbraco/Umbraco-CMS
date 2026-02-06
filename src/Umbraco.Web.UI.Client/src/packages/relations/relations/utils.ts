import type { UmbReferenceModel } from './types.js';
import type {
	IReferenceResponseModelDefaultReferenceResponseModel as DefaultReferenceResponseModel,
	IReferenceResponseModelDocumentReferenceResponseModel as DocumentReferenceResponseModel,
	IReferenceResponseModelMediaReferenceResponseModel as MediaReferenceResponseModel,
	IReferenceResponseModelMemberReferenceResponseModel as MemberReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

/**
 *
 * @param item
 */
export function isDocumentReference(item: UmbReferenceModel): item is DocumentReferenceResponseModel {
	return typeof (item as DocumentReferenceResponseModel).documentType !== 'undefined';
}

/**
 *
 * @param item
 */
export function isMediaReference(item: UmbReferenceModel): item is MediaReferenceResponseModel {
	return typeof (item as MediaReferenceResponseModel).mediaType !== 'undefined';
}

/**
 *
 * @param item
 */
export function isMemberReference(item: UmbReferenceModel): item is MemberReferenceResponseModel {
	return typeof (item as MemberReferenceResponseModel).memberType !== 'undefined';
}

/**
 *
 * @param item
 */
export function isDefaultReference(item: UmbReferenceModel): item is DefaultReferenceResponseModel {
	return typeof (item as DefaultReferenceResponseModel).type !== 'undefined';
}
