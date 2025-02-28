import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestContextualUserPermission } from '@umbraco-cms/backoffice/user-permission';

@customElement('umb-user-group-contextual-user-permission-list')
export class UmbUserGroupContextualUserPermissionListElement extends UmbLitElement {
	@state()
	private _groups: Array<string> = [];

	constructor() {
		super();
		this.#observeContextualUserPermissionGroups();
	}

	#observeContextualUserPermissionGroups() {
		this.observe(
			umbExtensionsRegistry.byType('contextualUserPermission'),
			(manifests) => {
				this._groups = [...new Set(manifests.flatMap((manifest) => manifest.meta.group))];
			},
			'umbContextualUserPermissionObserver',
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
				type="contextualUserPermission"
				default-element="umb-input-contextual-user-permission"
				.filter=${(manifest: ManifestContextualUserPermission) => manifest.meta.group === group}></umb-extension-slot>
		`;
	}

	static override styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-contextual-user-permission-list': UmbUserGroupContextualUserPermissionListElement;
	}
}
