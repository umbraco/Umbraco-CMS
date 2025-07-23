import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestExtensionUserPermission } from '../../user-extension-permission.extension.js';
import { UmbInputUserPermissionBaseElement } from '../input-user-permission-base.element.js';

@customElement('umb-input-extension-user-permission')
export class UmbInputExtensionUserPermissionElement extends UmbInputUserPermissionBaseElement<ManifestExtensionUserPermission> {
	@property({ type: String, attribute: false })
	forExtension: string = '';

	observePermissions() {
		this.manifestObserver?.destroy();

		this.manifestObserver = this.observe(
			umbExtensionsRegistry.byTypeAndFilter('extensionUserPermission', (m) => m.forExtension === this.forExtension),
			(userPermissionManifests) => {
				this.manifests = userPermissionManifests.filter((manifest) =>
					manifest.forEntityTypes.includes(this.entityType),
				);
			},
			'umbExtensionPermissionManifestsObserver',
		);
	}
}

export default UmbInputExtensionUserPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-extension-user-permission': UmbInputExtensionUserPermissionElement;
	}
}
