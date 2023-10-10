import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { ManifestUserPermission, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
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
				umbExtensionsRegistry.extensionsOfType('userPermission').pipe(
					map((manifests) => {
						return manifests.filter((manifest) => manifest.alias && value.includes(manifest.alias));
					}),
				),
				(userPermissionManifests) => this.#setUserPermissionLabels(userPermissionManifests),
				'userPermissionLabels',
			);
		} else {
			this.removeControllerByAlias('userPermissionLabels');
		}
	}

	#setUserPermissionLabels(manifests: Array<ManifestUserPermission>) {
		this.#userPermissionLabels = manifests.map((manifest) =>
			manifest.meta.labelKey ? this.localize.term(manifest.meta.labelKey) : manifest.meta.label ?? '',
		);
	}

	protected renderDetail() {
		const details: string[] = [];

		if (this.#userPermissionLabels.length > 0) {
			details.push(this.#userPermissionLabels.join(', '));
		}

		if (this.detail !== '') {
			details.push(this.detail);
		}

		return html`<small id="detail">${details.join(' | ')}<slot name="detail"></slot></small>`;
	}

	static styles = [...UUIRefNodeElement.styles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-ref': UmbUserGroupRefElement;
	}
}
