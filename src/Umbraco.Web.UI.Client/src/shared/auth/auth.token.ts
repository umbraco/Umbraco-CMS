import { IUmbAuth } from './auth.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_AUTH = new UmbContextToken<IUmbAuth>(
	'UmbAuth',
	'An instance of UmbAuthFlow that should be shared across the app.'
);
