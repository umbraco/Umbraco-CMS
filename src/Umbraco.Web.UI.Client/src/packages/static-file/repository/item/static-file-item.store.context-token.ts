import type UmbStaticFileItemStore from './static-file-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_STATIC_FILE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbStaticFileItemStore>('UmbStaticFileItemStore');
