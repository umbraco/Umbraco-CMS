import { UmbAuth } from './auth.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_AUTH_CONTEXT = new UmbContextToken<UmbAuth>(
	'UmbAuthContext'
);
