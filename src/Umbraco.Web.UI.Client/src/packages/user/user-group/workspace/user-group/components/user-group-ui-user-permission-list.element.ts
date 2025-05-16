import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestUiUserPermission } from '@umbraco-cms/backoffice/user-permission';

@customElement('umb-user-group-ui-user-permission-list')
export class UmbUserGroupUiUserPermissionListElement extends UmbLitElement {
	@state()
	private _groups: Array<string> = [];

	constructor() {
		super();
		this.#observeUiUserPermissionGroups();
	}

	#observeUiUserPermissionGroups() {
		this.observe(
			umbExtensionsRegistry.byType('uiUserPermission'),
			(manifests) => {
				this._groups = [...new Set(manifests.flatMap((manifest) => manifest.meta.group))];
			},
			'umbUiUserPermissionObserver',
		);
	}

	override render() {
		return html` ${this._groups.map((group) => this.#renderPermissionsByGroup(group))} `;
	}

	#renderPermissionsByGroup(group: string) {
		return html`
			<h4>${group}</h4>
			<umb-extension-slot
				slot="editor"
				type="uiUserPermission"
				default-element="umb-input-ui-user-permission"
				.filter=${(manifest: ManifestUiUserPermission) => manifest.meta.group === group}></umb-extension-slot>
		`;
	}

	static override styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-ui-user-permission-list': UmbUserGroupUiUserPermissionListElement;
	}
}
