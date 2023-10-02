import { UmbChangeEvent, UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/events';
import {
	ManifestEntityAction,
	ManifestUserPermission,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { css, html, customElement, property, state, nothing, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { groupBy } from '@umbraco-cms/backoffice/external/lodash';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbUserPermissionSettingElement } from '@umbraco-cms/backoffice/users';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-entity-user-permission-settings-list')
export class UmbEntityUserPermissionSettingsListElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		if (value === this._entityType) return;
		this._entityType = value;
		this.#observeUserPermissions();
	}
	private _entityType: string = '';

	@property({ attribute: false })
	selectedPermissions: Array<string> = [];

	@state()
	private _manifests: Array<ManifestUserPermission> = [];

	#manifestObserver?: UmbObserverController<Array<ManifestUserPermission>>;

	#isAllowed(permissionAlias: string) {
		return this.selectedPermissions?.includes(permissionAlias);
	}

	#observeUserPermissions() {
		this.#manifestObserver?.destroy();

		this.#manifestObserver = this.observe(
			umbExtensionsRegistry.extensionsOfType('userPermission'),
			(userPermissionManifests) => {
				this._manifests = userPermissionManifests.filter((manifest) => manifest.meta.entityType === this.entityType);
			},
		);
	}

	#onChangeUserPermission(event: UmbChangeEvent, permissionAlias: string) {
		event.stopPropagation();
		const target = event.target as UmbUserPermissionSettingElement;
		target.allowed ? this.#addUserPermission(permissionAlias) : this.#removeUserPermission(permissionAlias);
	}

	#addUserPermission(permissionAlias: string) {
		this.selectedPermissions = [...this.selectedPermissions, permissionAlias];
		this.dispatchEvent(new UmbSelectionChangeEvent());
	}

	#removeUserPermission(permissionAlias: string) {
		this.selectedPermissions = this.selectedPermissions.filter((alias) => alias !== permissionAlias);
		this.dispatchEvent(new UmbSelectionChangeEvent());
	}

	render() {
		return html`${this.#renderGroupedPermissions(this._manifests)} `;
	}

	#renderGroupedPermissions(permissionManifests: Array<ManifestUserPermission>) {
		const groupedPermissions = groupBy(permissionManifests, (manifest) => manifest.meta.group);
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

	#renderPermission(manifest: ManifestUserPermission) {
		return html` <umb-user-permission-setting
			label=${ifDefined(manifest.meta.labelKey ? this.localize.term(manifest.meta.labelKey) : manifest.meta.label)}
			description=${ifDefined(
				manifest.meta.descriptionKey ? this.localize.term(manifest.meta.descriptionKey) : manifest.meta.description,
			)}
			?allowed=${this.#isAllowed(manifest.alias)}
			@change=${(event: UmbChangeEvent) =>
				this.#onChangeUserPermission(event, manifest.alias)}></umb-user-permission-setting>`;
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this.#manifestObserver?.destroy();
	}

	static styles = [css``];
}

export default UmbEntityUserPermissionSettingsListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-user-permission-settings-list': UmbEntityUserPermissionSettingsListElement;
	}
}
