import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestGranularUserPermission } from '@umbraco-cms/backoffice/user-permission';
import { html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { filterFrozenArray } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-user-group-granular-permission-list')
export class UmbUserGroupGranularPermissionListElement extends UmbLitElement {
	@state()
	_userGroupPermissions?: Array<any>;

	@state()
	_userGroupFallbackPermissions?: Array<string>;

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;

			this.observe(
				this.#workspaceContext?.data,
				(userGroup) => {
					this._userGroupPermissions = userGroup?.permissions;
					this._userGroupFallbackPermissions = userGroup?.fallbackPermissions;
				},
				'umbUserGroupGranularPermissionObserver',
			);
		});
	}

	#onValueChange = (e: UmbChangeEvent) => {
		e.stopPropagation();
		// TODO: make interface
		const target = e.target as any;
		const schemaType = target.manifest?.meta.schemaType;
		if (!schemaType) throw new Error('Schema type is not available');

		/* Remove all permissions of the same schema type from
		the user group and append the new permissions.
		We do it this way to support appends, updates and deletion without we know the
		exact action but on the changed value */
		const storedValueWithoutSchemaTypeItems = filterFrozenArray(
			this.#workspaceContext?.getPermissions() || [],
			(x) => x.$type !== schemaType,
		);

		const permissions = target.permissions || [];
		const newCombinedValue = [...storedValueWithoutSchemaTypeItems, ...permissions];

		this.#workspaceContext?.setPermissions(newCombinedValue);
	};

	override render() {
		if (!this._userGroupPermissions) return;
		return html`<umb-extension-slot
			type="userGranularPermission"
			.props=${{ fallbackPermissions: this._userGroupFallbackPermissions }}
			.renderMethod=${this.#renderProperty}></umb-extension-slot>`;
	}

	#renderProperty = (extension: UmbExtensionElementInitializer<ManifestGranularUserPermission>) => {
		if (!this._userGroupPermissions) return nothing;
		if (!extension.component) return nothing;

		const manifest = extension.manifest;
		if (!manifest) throw new Error('Manifest is not available');

		const label = manifest.meta.labelKey ? this.localize.term(manifest.meta.labelKey) : manifest.meta.label;
		const description = manifest.meta.descriptionKey
			? this.localize.term(manifest.meta.descriptionKey)
			: manifest.meta.description;

		const schemaType = manifest.meta.schemaType;
		const permissionsForSchemaType =
			this._userGroupPermissions.filter((permission) => permission.$type === schemaType) || [];

		(extension.component as any).permissions = permissionsForSchemaType;
		(extension.component as any).fallbackPermissions = this._userGroupFallbackPermissions;
		extension.component.addEventListener(UmbChangeEvent.TYPE, this.#onValueChange);

		return html`
			<umb-property-layout .label=${label || ''} .description=${description || ''}>
				<div slot="editor">${extension.component}</div>
			</umb-property-layout>
		`;
	};
}

export default UmbUserGroupGranularPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-granular-permission-list': UmbUserGroupGranularPermissionListElement;
	}
}
