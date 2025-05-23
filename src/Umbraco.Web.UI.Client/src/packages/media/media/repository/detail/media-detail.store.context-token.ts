import type UmbMediaDetailStore from './media-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMediaDetailStore>('UmbMediaDetailStore');
