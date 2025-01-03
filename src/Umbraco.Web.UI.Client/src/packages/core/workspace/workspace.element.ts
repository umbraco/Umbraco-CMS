import { nothing, customElement, property, type PropertyValueMap, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/workspace';
import {
	UmbExtensionsApiInitializer,
	UmbExtensionsElementAndApiInitializer,
	type UmbApiConstructorArgumentsMethodType,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const apiArgsCreator: UmbApiConstructorArgumentsMethodType<unknown> = (manifest: unknown) => {
	return [{ manifest }];
};

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementAndApiInitializer<any>;
	#entityType?: string;

	@state()
	_component?: HTMLElement;

	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string | undefined {
		return this.#entityType;
	}
	public set entityType(value: string) {
		if (value === this.#entityType || !value) return;
		this.#entityType = value;
		this.#createController(value);
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'workspace');
	}

	#createController(entityType: string) {
		if (this.#extensionsController) {
			this.#extensionsController.destroy();
		}
		this.#extensionsController = new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'workspace',
			apiArgsCreator,
			(manifest: ManifestWorkspace) => manifest.meta.entityType === entityType,
			(extensionControllers) => {
				this._component = extensionControllers[0]?.component;
				const api = extensionControllers[0]?.api;
				if (api) {
					// We create the additional workspace contexts with the Workspace API as its host, to ensure they can use the same Context-Alias with different API-Aliases and still be reached cause they will then be provided at the same host. [NL]
					new UmbExtensionsApiInitializer(api, umbExtensionsRegistry, 'workspaceContext', [api]);
				}
			},
			undefined, // We can leave the alias to undefined, as we destroy this our selfs.
			undefined,
			() => import('./kinds/default/default-workspace.context.js'),
			{ single: true },
		);
	}

	override render() {
		return this._component ?? nothing;
	}
}

export { UmbWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace': UmbWorkspaceElement;
	}
}
