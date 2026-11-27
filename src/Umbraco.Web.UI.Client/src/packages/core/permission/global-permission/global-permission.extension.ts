import type { UmbPermissionDenyReason } from '../types.js';
import type { ManifestBase, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * Allows or denies permission verbs everywhere while its conditions are met. The conditions run on an app-wide host,
 * so only conditions that need no workspace or entity context can be met. A deny only hides UI; the server must deny
 * the verb as well.
 */
export interface ManifestGlobalPermission
	extends ManifestBase, ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'globalPermission';
	meta: MetaGlobalPermission;
}

export interface MetaGlobalPermission {
	/**
	 * The verbs to allow.
	 * @example ["Umb.DocumentType.Create", "Umb.DocumentType.Update"]
	 */
	allow?: Array<string>;
	/**
	 * The verbs to deny. A deny wins over every allow.
	 * @example ["Umb.DocumentType.Delete"]
	 */
	deny?: Array<string>;
	/** Why the verbs in `deny` are denied. */
	denyReason?: UmbPermissionDenyReason;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbGlobalPermission: ManifestGlobalPermission;
	}
}
