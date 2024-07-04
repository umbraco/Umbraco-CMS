import type UmbMediaTypeDetailStore from './media-type-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeDetailStore>(
	'UmbMediaTypeDetailStore',
);
