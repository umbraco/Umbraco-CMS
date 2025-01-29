import type UmbPackageStore from './package.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PACKAGE_STORE_TOKEN = new UmbContextToken<UmbPackageStore>('UmbPackageStore');
