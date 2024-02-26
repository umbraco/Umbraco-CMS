import type { UmbBlockGridAreaTypeWorkspaceContext } from './block-grid-area-type-workspace.context.js';
import { UmbBlockGridAreaTypeWorkspaceEditorElement } from './block-grid-area-type-workspace-editor.element.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsApiInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-block-grid-area-type-workspace')
export class UmbBlockGridAreaTypeWorkspaceElement extends UmbLitElement {
	//
	#manifest?: ManifestWorkspace;
	#workspaceContext?: UmbBlockGridAreaTypeWorkspaceContext;
	#editorElement = () => {
		const element = new UmbBlockGridAreaTypeWorkspaceEditorElement();
		element.workspaceAlias = this.#manifest!.alias;
		return element;
	};

	@state()
	_routes: UmbRoute[] = [];

	public set manifest(manifest: ManifestWorkspace) {
		this.#manifest = manifest;
		createExtensionApi(manifest, [this, { manifest: manifest }]).then((context) => {
			if (context) {
				this.#gotWorkspaceContext(context);
			}
		});
	}

	#gotWorkspaceContext(context: UmbApi) {
		this.#workspaceContext = context as UmbBlockGridAreaTypeWorkspaceContext;

		this._routes = [
			/*
			{
				// Would it make more sense to have groupKey before elementTypeKey?
				path: 'create',
				component: this.#editorElement,
				setup: async (_component, info) => {
					this.#workspaceContext!.create();

					new UmbWorkspaceIsNewRedirectController(
						this,
						this.#workspaceContext!,
						this.shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},*/
			{
				path: 'edit/:id',
				component: this.#editorElement,
				setup: (_component, info) => {
					const id = info.match.params.id;
					this.#workspaceContext!.load(id);
				},
			},
		];

		// TODO: We need to recreate when ID changed?
		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [this, this.#workspaceContext]);
	}

	render() {
		return html`<umb-router-slot .routes="${this._routes}"></umb-router-slot>`;
	}
}

export default UmbBlockGridAreaTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-area-type-workspace': UmbBlockGridAreaTypeWorkspaceElement;
	}
}
