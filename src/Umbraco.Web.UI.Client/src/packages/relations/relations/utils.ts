import type { UmbReferenceModel } from './types.js';
import type {
	IReferenceResponseModelDefaultReferenceResponseModel as DefaultReferenceResponseModel,
	IReferenceResponseModelDocumentReferenceResponseModel as DocumentReferenceResponseModel,
	IReferenceResponseModelMediaReferenceResponseModel as MediaReferenceResponseModel,
	IReferenceResponseModelMemberReferenceResponseModel as MemberReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Determines whether the given reference is a document reference.
 * @param {UmbReferenceModel} item - The reference to check.
 * @returns {boolean} `true` if the item is a document reference.
 */
export function isDocumentReference(item: UmbReferenceModel): item is DocumentReferenceResponseModel {
	return typeof (item as DocumentReferenceResponseModel).documentType !== 'undefined';
}

/**
 * Determines whether the given reference is a media reference.
 * @param {UmbReferenceModel} item - The reference to check.
 * @returns {boolean} `true` if the item is a media reference.
 */
export function isMediaReference(item: UmbReferenceModel): item is MediaReferenceResponseModel {
	return typeof (item as MediaReferenceResponseModel).mediaType !== 'undefined';
}

/**
 * Determines whether the given reference is a member reference.
 * @param {UmbReferenceModel} item - The reference to check.
 * @returns {boolean} `true` if the item is a member reference.
 */
export function isMemberReference(item: UmbReferenceModel): item is MemberReferenceResponseModel {
	return typeof (item as MemberReferenceResponseModel).memberType !== 'undefined';
}

/**
 * Determines whether the given reference is a default reference.
 * @param {UmbReferenceModel} item - The reference to check.
 * @returns {boolean} `true` if the item is a default reference.
 */
export function isDefaultReference(item: UmbReferenceModel): item is DefaultReferenceResponseModel {
	return typeof (item as DefaultReferenceResponseModel).type !== 'undefined';
}
