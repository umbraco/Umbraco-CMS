import type { UmbReferenceModel } from './types.js';
import type {
	DefaultReferenceResponseModel,
	DocumentReferenceResponseModel,
	MediaReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export function isDocumentReference(item: UmbReferenceModel): item is DocumentReferenceResponseModel {
	return typeof (item as DocumentReferenceResponseModel).documentType !== 'undefined';
}

export function isMediaReference(item: UmbReferenceModel): item is MediaReferenceResponseModel {
	return typeof (item as MediaReferenceResponseModel).mediaType !== 'undefined';
}

export function isDefaultReference(item: UmbReferenceModel): item is DefaultReferenceResponseModel {
	return typeof (item as DefaultReferenceResponseModel).type !== 'undefined';
}
