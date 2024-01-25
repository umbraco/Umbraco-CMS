import type { UmbAuthContext } from './auth.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_AUTH_CONTEXT = new UmbContextToken<UmbAuthContext>('UmbAuthContext');
