import { IUmbAuth } from './auth.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_AUTH = new UmbContextToken<IUmbAuth>(
	'UmbAuth'
);
