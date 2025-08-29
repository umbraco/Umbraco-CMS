import type { ManifestEntityUserPermission } from '../../entity-user-permission.extension.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbInputUserPermissionBaseElement } from '../input-user-permission-base.element.js';

@customElement('umb-input-entity-user-permission')
export class UmbInputEntityUserPermissionElement extends UmbInputUserPermissionBaseElement<ManifestEntityUserPermission> {

	observePermissions() {
		this.manifestObserver?.destroy();

		this.manifestObserver = this.observe(
			umbExtensionsRegistry.byType('entityUserPermission'),
			(userPermissionManifests) => {
				this.manifests = userPermissionManifests.filter((manifest) =>
					manifest.forEntityTypes.includes(this.entityType),
				);
			},
			'umbUserPermissionManifestsObserver',
		);
	}
}

export default UmbInputEntityUserPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-entity-user-permission': UmbInputEntityUserPermissionElement;
	}
}
