import type { ManifestClass } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestGlobalContext extends ManifestClass {
	type: 'globalContext';
}
