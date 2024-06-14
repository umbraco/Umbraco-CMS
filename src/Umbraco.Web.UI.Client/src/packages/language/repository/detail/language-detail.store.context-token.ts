import type { UmbLanguageDetailStore } from './language-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_LANGUAGE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbLanguageDetailStore>('UmbLanguageDetailStore');
