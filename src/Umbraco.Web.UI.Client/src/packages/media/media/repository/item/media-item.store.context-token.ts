import type UmbMediaItemStore from './media-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMediaItemStore>('UmbMediaItemStore');
