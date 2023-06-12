import { IUmbAuth } from './auth.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export type { IUmbAuth } from './auth.interface.js';
export { UmbAuthFlow } from './auth-flow.js';
export { UmbAuthStore } from './auth.store.js';

export * from './types.js';

export const UMB_AUTH = new UmbContextToken<IUmbAuth>(
	'UmbAuth',
	'An instance of UmbAuthFlow that should be shared across the app.'
);
