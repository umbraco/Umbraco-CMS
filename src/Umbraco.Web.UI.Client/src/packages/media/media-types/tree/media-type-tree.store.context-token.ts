import type UmbMediaTypeTreeStore from './media-type-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeTreeStore>('UmbMediaTypeTreeStore');
