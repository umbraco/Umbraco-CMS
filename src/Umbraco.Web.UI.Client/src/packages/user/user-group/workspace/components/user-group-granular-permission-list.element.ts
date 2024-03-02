import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestGranularUserPermission } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { filterFrozenArray } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-user-group-granular-permission-list')
export class UmbUserGroupGranularPermissionListElement extends UmbLitElement {
	@state()
	_extensionElements: Array<HTMLElement> = [];

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
		});

		this.#observeExtensionRegistry();
	}

	#observeExtensionRegistry() {
		this.observe(umbExtensionsRegistry.byType('userGranularPermission'), (manifests) => {
			if (!manifests) {
				this._extensionElements = [];
				return;
			}

			manifests.forEach(async (manifest) => this.#extensionElementSetup(manifest));
		});
	}

	async #extensionElementSetup(manifest: ManifestGranularUserPermission) {
		const element = (await createExtensionElement(manifest)) as any;
		if (!element) throw new Error(`Failed to create extension element for manifest ${manifest.alias}`);
		if (!this.#workspaceContext) throw new Error('User Group Workspace context is not available');

		this.observe(
			this.#workspaceContext.data,
			(userGroup) => {
				if (!userGroup) return;

				const schemaType = manifest.meta.schemaType;
				const permissionsForSchemaType =
					userGroup.permissions.filter((permission) => permission.$type === schemaType) || [];

				element.permissions = permissionsForSchemaType;
				element.manifest = manifest;
				element.addEventListener(UmbChangeEvent.TYPE, this.#onValueChange);
			},
			'umbUserGroupGranularPermissionObserver',
		);

		this._extensionElements.push(element);
		this.requestUpdate('_extensionElements');
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

	render() {
		return html`${this._extensionElements.map((element) => this.#renderProperty(element))}`;
	}

	#renderProperty(element: any) {
		const manifest = element.manifest as ManifestGranularUserPermission;
		const label = manifest.meta.labelKey ? this.localize.term(manifest.meta.labelKey) : manifest.meta.label;
		const description = manifest.meta.descriptionKey
			? this.localize.term(manifest.meta.descriptionKey)
			: manifest.meta.description;

		return html`
			<umb-property-layout .label=${label || ''} .description=${description || ''}>
				<div slot="editor">${element}</div>
			</umb-property-layout>
		`;
	}

	disconnectedCallback(): void {
		this._extensionElements.forEach((element) => element.removeEventListener(UmbChangeEvent.TYPE, this.#onValueChange));
		super.disconnectedCallback();
	}
}

export default UmbUserGroupGranularPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-granular-permission-list': UmbUserGroupGranularPermissionListElement;
	}
}
