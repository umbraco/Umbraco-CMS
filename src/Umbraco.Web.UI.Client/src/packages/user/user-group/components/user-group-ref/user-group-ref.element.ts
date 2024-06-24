import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { ManifestEntityUserPermission } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { map } from '@umbraco-cms/backoffice/external/rxjs';

/**
 *  @element umb-user-group-ref
 *  @description - Component for displaying a reference to a User Group
 *  @extends UUIRefNodeElement
 */
@customElement('umb-user-group-ref')
export class UmbUserGroupRefElement extends UmbElementMixin(UUIRefNodeElement) {
	@property({ type: Array, attribute: 'user-permission-aliases' })
	public get userPermissionAliases(): Array<string> {
		return [];
	}
	public set userPermissionAliases(value: Array<string>) {
		this.#observeUserPermissions(value);
	}

	#userPermissionLabels: Array<string> = [];

	async #observeUserPermissions(value: Array<string>) {
		if (value) {
			this.observe(
				umbExtensionsRegistry.byType('entityUserPermission').pipe(
					map((manifests) => {
						return manifests.filter((manifest) => manifest.alias && value.includes(manifest.alias));
					}),
				),
				(userPermissionManifests) => this.#setUserPermissionLabels(userPermissionManifests),
				'userPermissionLabels',
			);
		} else {
			this.removeUmbControllerByAlias('userPermissionLabels');
		}
	}

	#setUserPermissionLabels(manifests: Array<ManifestEntityUserPermission>) {
		this.#userPermissionLabels = manifests.map((manifest) =>
			manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name,
		);
	}

	protected override renderDetail() {
		const details: string[] = [];

		if (this.#userPermissionLabels.length > 0) {
			details.push(this.#userPermissionLabels.join(', '));
		}

		if (this.detail !== '') {
			details.push(this.detail);
		}

		return html`<small id="detail">${details.join(' | ')}<slot name="detail"></slot></small>`;
	}

	static override styles = [...UUIRefNodeElement.styles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-ref': UmbUserGroupRefElement;
	}
}
