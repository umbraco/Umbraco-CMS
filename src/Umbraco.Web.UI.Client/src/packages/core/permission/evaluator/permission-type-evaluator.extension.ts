import type { UmbPermissionTypeEvaluatorApi } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPermissionTypeEvaluator extends ManifestApi<UmbPermissionTypeEvaluatorApi> {
	type: 'permissionTypeEvaluator';
	/** The type of the permissions it receives. */
	forPermissionType: string;
	/**
	 * Every verb it may give a verdict for. The registry only creates it when one of these is checked, and ignores
	 * verdicts for verbs not listed. Listing too many only costs performance.
	 */
	forPermissionVerbs: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbPermissionTypeEvaluator: ManifestPermissionTypeEvaluator;
	}
}
