import type {
	ContentTreeItemResponseModel,
	ContentUrlInfoModel,
	MediaTypeReferenceResponseModel,
	MediaValueModel,
	MediaVariantResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export * from './components/index.js';
export * from './repository/index.js';

export { UMB_MEDIA_COLLECTION_ALIAS } from './collection/index.js';

// Content
export interface ContentProperty {
	alias: string;
	label: string;
	description: string;
	dataTypeId: string;
}
export interface UmbMediaDetailModel {
	id: string;
	isTrashed: boolean;
	variants: Array<MediaVariantResponseModel>;
	values: Array<MediaValueModel>;
	urls: Array<ContentUrlInfoModel>;
	mediaType: MediaTypeReferenceResponseModel;
}
