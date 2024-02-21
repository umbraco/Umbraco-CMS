import type { UmbUserPermissionModel } from '../../types.js';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import type { ManifestEntityUserPermission } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state, nothing, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { UmbUserPermissionSettingElement } from '@umbraco-cms/backoffice/user';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-user-permission-settings-list')
export class UmbEntityUserPermissionSettingsListElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		if (value === this._entityType) return;
		this._entityType = value;
		this.#observeEntityUserPermissions();
	}
	private _entityType: string = '';

	@property({ attribute: false })
	selectedPermissions: Array<UmbUserPermissionModel> = [];

	@state()
	private _manifests: Array<ManifestEntityUserPermission> = [];

	#manifestObserver?: UmbObserverController<Array<ManifestEntityUserPermission>>;

	#isAllowed(permissionVerbs: Array<string>) {
		const permission = { verbs: permissionVerbs };
		const permissionAsString = JSON.stringify(permission);
		return this.selectedPermissions?.map((p) => JSON.stringify(p)).includes(permissionAsString);
	}

	#observeEntityUserPermissions() {
		this.#manifestObserver?.destroy();

		this.#manifestObserver = this.observe(
			umbExtensionsRegistry.byType('entityUserPermission'),
			(userPermissionManifests) => {
				this._manifests = userPermissionManifests.filter((manifest) => manifest.meta.entityType === this.entityType);
			},
			'umbUserPermissionManifestsObserver',
		);
	}

	#onChangeUserPermission(event: UmbChangeEvent, permissionVerbs: Array<string>) {
		event.stopPropagation();
		const target = event.target as UmbUserPermissionSettingElement;
		target.allowed ? this.#addUserPermission(permissionVerbs) : this.#removeUserPermission(permissionVerbs);
	}

	#addUserPermission(permissionVerbs: Array<string>) {
		const newUserPermission: UmbUserPermissionModel = { verbs: permissionVerbs };
		this.selectedPermissions = [...this.selectedPermissions, newUserPermission];
		this.dispatchEvent(new UmbSelectionChangeEvent());
	}

	#removeUserPermission(permissionVerbs: Array<string>) {
		// We only want to remove the global permission and not any granular permissions with the same verb
		// because we don't know what models can be part of the array we will make a string comparison
		const permission: UmbUserPermissionModel = { verbs: permissionVerbs };
		const permissionAsString = JSON.stringify(permission);

		this.selectedPermissions = this.selectedPermissions
			.map((p) => JSON.stringify(p))
			.filter((p) => p !== permissionAsString)
			.map((p) => JSON.parse(p));
		this.dispatchEvent(new UmbSelectionChangeEvent());
	}

	render() {
		return html`${this.#renderGroupedPermissions(this._manifests)} `;
	}

	#renderGroupedPermissions(permissionManifests: Array<ManifestEntityUserPermission>) {
		// TODO: groupBy is not known by TS yet
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-expect-error
		const groupedPermissions = Object.groupBy(
			permissionManifests,
			(manifest: ManifestEntityUserPermission) => manifest.meta.group,
		) as Record<string, Array<ManifestEntityUserPermission>>;
		return html`
			${Object.entries(groupedPermissions).map(
				([group, manifests]) => html`
					${group !== 'undefined'
						? html` <h5><umb-localize .key=${`actionCategories_${group}`}>${group}</umb-localize></h5> `
						: nothing}
					${manifests.map((manifest) => html` ${this.#renderPermission(manifest)} `)}
				`,
			)}
		`;
	}

	#renderPermission(manifest: ManifestEntityUserPermission) {
		return html` <umb-user-permission-setting
			label=${ifDefined(manifest.meta.labelKey ? this.localize.term(manifest.meta.labelKey) : manifest.meta.label)}
			description=${ifDefined(
				manifest.meta.descriptionKey ? this.localize.term(manifest.meta.descriptionKey) : manifest.meta.description,
			)}
			?allowed=${this.#isAllowed(manifest.meta.verbs)}
			@change=${(event: UmbChangeEvent) =>
				this.#onChangeUserPermission(event, manifest.meta.verbs)}></umb-user-permission-setting>`;
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this.#manifestObserver?.destroy();
	}
}

export default UmbEntityUserPermissionSettingsListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-user-permission-settings-list': UmbEntityUserPermissionSettingsListElement;
	}
}
