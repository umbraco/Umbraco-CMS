import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context.js';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestGranularUserPermission } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

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

				element.value = permissionsForSchemaType;
			},
			'umbUserGroupPermissionObserver',
		);

		this._extensionElements.push(element);
		this.requestUpdate('_extensionElements');
	}

	render() {
		return html`${this._extensionElements.map((element) => html`${element}`)}`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbUserGroupGranularPermissionListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-granular-permission-list': UmbUserGroupGranularPermissionListElement;
	}
}
