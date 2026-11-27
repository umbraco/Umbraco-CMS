import { UMB_GLOBAL_PERMISSION_TYPE } from '../constants.js';
import { umbPermissionRegistry } from '../registry/permission.registry.js';
import type { UmbPermission } from '../types.js';
import { UMB_GLOBAL_PERMISSION_MANIFEST_SOURCE_ALIAS } from './constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * Sets the `globalPermission` manifests whose conditions are met as `'global'` permissions in the permission registry.
 */
export class UmbGlobalPermissionManifestSource extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);

		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'globalPermission', null, (permitted) => {
			umbPermissionRegistry.setSource(
				UMB_GLOBAL_PERMISSION_MANIFEST_SOURCE_ALIAS,
				permitted.map(({ manifest }): UmbPermission => {
					const { allow, deny, denyReason } = manifest.meta;
					return { type: UMB_GLOBAL_PERMISSION_TYPE, allow, deny, denyReason };
				}),
			);
		});
	}

	override destroy(): void {
		super.destroy();
		umbPermissionRegistry.removeSource(UMB_GLOBAL_PERMISSION_MANIFEST_SOURCE_ALIAS);
	}
}

export { UmbGlobalPermissionManifestSource as api };
