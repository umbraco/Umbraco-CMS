import type { UmbAuthContext } from './auth.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_AUTH_CONTEXT = new UmbContextToken<UmbAuthContext>('UmbAuthContext');
export const UMB_STORAGE_TOKEN_RESPONSE_NAME = 'umb:userAuthTokenResponse';
export const UMB_STORAGE_REDIRECT_URL = 'umb:auth:redirect';
