import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbUserGroupPermissionsListBaseElement } from './user-group-permission-list-base.element.js';

@customElement('umb-user-group-entity-user-permission-list')
export class UmbUserGroupEntityUserPermissionListElement extends UmbUserGroupPermissionsListBaseElement {
	@state()
	private _groups: Array<{ entityType: string; headline: string }> = [];

	constructor() {
		super();

		this.#observeEntityUserPermissions();
	}

	#observeEntityUserPermissions() {
		this.observe(
			umbExtensionsRegistry.byType('entityUserPermission'),
			(manifests) => {
				const entityTypes = [...new Set(manifests.flatMap((manifest) => manifest.forEntityTypes))];
				this._groups = entityTypes
					.map((entityType) => {
						return {
							entityType,
							headline: this.localize.term(`user_permissionsEntityGroup_${entityType}`),
						};
					})
					.sort((a, b) => a.headline.localeCompare(b.headline));
			},
			'umbUserPermissionsObserver',
		);
	}

	override render() {
		return html` ${this._groups.map((group) => this.#renderPermissionsForEntityType(group))}`;
	}

	#renderPermissionsForEntityType(group: { entityType: string; headline: string }) {
		return html`
			<h4>${group.headline}</h4>
			<umb-input-entity-user-permission
				.entityType=${group.entityType}
				.allowedVerbs=${this._fallBackPermissions || []}
				@change=${this.onPermissionChange}></umb-input-entity-user-permission>
		`;
	}
}

export default UmbUserGroupEntityUserPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-default-permission-list': UmbUserGroupEntityUserPermissionListElement;
	}
}
