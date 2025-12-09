import { dataSet } from './sets/index.js';
import type {
	DefaultReferenceResponseModel,
	DocumentReferenceResponseModel,
	MediaReferenceResponseModel,
	MemberReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const items: Array<
	| DefaultReferenceResponseModel
	| DocumentReferenceResponseModel
	| MediaReferenceResponseModel
	| MemberReferenceResponseModel
> = dataSet.trackedReferenceItems;
