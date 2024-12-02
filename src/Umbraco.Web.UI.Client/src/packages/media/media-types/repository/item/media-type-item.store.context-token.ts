import type UmbMediaTypeItemStore from './media-type-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeItemStore>('UmbMediaTypeItemStore');
