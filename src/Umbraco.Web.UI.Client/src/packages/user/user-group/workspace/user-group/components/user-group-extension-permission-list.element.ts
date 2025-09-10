import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import type {
	ManifestExtensionPermissions,
	ManifestExtensionUserPermission,
} from '@umbraco-cms/backoffice/user-permission';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbUserGroupPermissionsListBaseElement } from './user-group-permission-list-base.element.js';

@customElement('umb-user-group-extension-permission-list')
export class UmbUserGroupExtensionPermissionsListElement extends UmbUserGroupPermissionsListBaseElement {
	@state()
	private _manifests?: ManifestExtensionUserPermission[];

	@state()
	private _extensionElements = new Map<string, HTMLElement>();

	@state()
	private _extensions?: ManifestExtensionPermissions[];

	constructor() {
		super();

		this.#observeExtensionPermissions();
		this.#observeEntityUserPermissions();
	}

	#observeExtensionPermissions() {
		this.observe(
			umbExtensionsRegistry.byType('extensionPermissions'),
			async (manifests) => {
				this._extensions = manifests;

				// Pre-create extension elements for each manifest
				for (const manifest of manifests) {
					if (!manifest.element && !manifest.js) continue;
					const element = await createExtensionElement(manifest);

					if (element) {
						this._extensionElements.set(manifest.meta.extensionAlias, element);
					}
				}
				this.requestUpdate('_extensionElements');
			},
			'umbExtensionPermissionsObserver',
		);
	}

	#observeEntityUserPermissions() {
		this.observe(
			umbExtensionsRegistry.byType('extensionUserPermission'),
			(manifests) => (this._manifests = manifests),
			'umbExtensionUserPermissionsObserver',
		);
	}

	#groupPermissionsForExtension(manifests: ManifestExtensionUserPermission[]) {
		const entityTypes = [...new Set(manifests.flatMap((manifest) => manifest.forEntityTypes))];

		return entityTypes
			.map((entityType) => ({
				entityType,
				headline: this.localize.term(`user_permissionsEntityGroup_${entityType}`),
			}))
			.sort((a, b) => a.headline.localeCompare(b.headline));
	}

	#renderProperty(manifest: ManifestExtensionPermissions) {
		const label = manifest.meta.labelKey ? this.localize.term(manifest.meta.labelKey) : manifest.meta.label;
		const description = manifest.meta.descriptionKey
			? this.localize.term(manifest.meta.descriptionKey)
			: manifest.meta.description;

		const permissions = this._manifests?.filter((x) => x.forExtension === manifest.meta.extensionAlias) || [];
		const groupedPermissions = this.#groupPermissionsForExtension(permissions);

		return html`
			<umb-property-layout .label=${label || ''} .description=${description || ''}>
				<div slot="editor">
					${this._extensionElements.get(manifest.meta.extensionAlias)}
					${groupedPermissions.map(
						(group) =>
							html` <h4>${group.headline}</h4>
								<umb-input-extension-user-permission
									.forExtension=${manifest.meta.extensionAlias}
									.entityType=${group.entityType}
									.allowedVerbs=${this._fallBackPermissions || []}
									@change=${this.onPermissionChange}></umb-input-extension-user-permission>`,
					)}
				</div>
			</umb-property-layout>
		`;
	}

	override render() {
		if (!this._extensions) return nothing;

		return html`${this._extensions.map((extension) => this.#renderProperty(extension))}`;
	}
}

export default UmbUserGroupExtensionPermissionsListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-extension-permission-list': UmbUserGroupExtensionPermissionsListElement;
	}
}
