import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state, repeat, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

import './user-group-entity-type-permissions.element.js';
import './user-group-entity-type-granular-permissions.element.js';

@customElement('umb-user-group-entity-type-permission-groups')
export class UmbUserGroupEntityTypePermissionGroupsElement extends UmbLitElement {
	@state()
	private _groups: Array<{ entityType: string; headline: string }> = [];

	constructor() {
		super();

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
		return html`${repeat(
			this._groups,
			(group) => group.entityType,
			(group) =>
				html`<uui-box>
					<div slot="headline">${group.headline}</div>

					<umb-user-group-entity-type-permissions
						.entityType=${group.entityType}></umb-user-group-entity-type-permissions>

					<umb-user-group-entity-type-granular-permissions
						.entityType=${group.entityType}></umb-user-group-entity-type-granular-permissions>
				</uui-box>`,
		)}`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-box {
				margin-top: var(--uui-size-space-6);
			}
		`,
	];
}

export default UmbUserGroupEntityTypePermissionGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-entity-type-permission-groups': UmbUserGroupEntityTypePermissionGroupsElement;
	}
}
