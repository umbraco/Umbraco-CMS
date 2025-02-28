import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestUserPermission } from '@umbraco-cms/backoffice/user-permission';

@customElement('umb-user-group-user-permission-list')
export class UmbUserGroupUserPermissionListElement extends UmbLitElement {
	@state()
	private _groups: Array<string> = [];

	constructor() {
		super();
		this.#observeUserPermissionGroups();
	}

	#observeUserPermissionGroups() {
		this.observe(
			umbExtensionsRegistry.byType('userPermission'),
			(manifests) => {
				this._groups = [...new Set(manifests.flatMap((manifest) => manifest.meta.group))];
			},
			'umbUserPermissionObserver',
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
				type="userPermission"
				default-element="umb-input-user-permission"
				.filter=${(manifest: ManifestUserPermission) => manifest.meta.group === group}></umb-extension-slot>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbUserGroupUserPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-user-permission-list': UmbUserGroupUserPermissionListElement;
	}
}
