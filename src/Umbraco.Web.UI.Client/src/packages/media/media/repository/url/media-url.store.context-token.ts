import type UmbMediaUrlStore from './media-url.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_URL_STORE_CONTEXT = new UmbContextToken<UmbMediaUrlStore>('UmbMediaUrlStore');
