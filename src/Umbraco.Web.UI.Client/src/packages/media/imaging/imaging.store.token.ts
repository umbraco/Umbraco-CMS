import type { UmbImagingStore } from './imaging.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_IMAGING_STORE_CONTEXT = new UmbContextToken<UmbImagingStore>('UmbImagingStore');
