import type { UmbBlockTypeWorkspaceContext } from './block-type-workspace.context.js';
import { UmbBlockTypeWorkspaceEditorElement } from './block-type-workspace-editor.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsApiInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-block-type-workspace')
export class UmbBlockTypeWorkspaceElement extends UmbLitElement {
	//
	#manifest?: ManifestWorkspace;
	#workspaceContext?: UmbBlockTypeWorkspaceContext;
	#editorElement = () => {
		const element = new UmbBlockTypeWorkspaceEditorElement();
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
		this.#workspaceContext = context as UmbBlockTypeWorkspaceContext;

		this._routes = [
			{
				// Would it make more sense to have groupKey before elementTypeKey?
				path: 'create/:elementTypeKey/:groupKey',
				component: this.#editorElement,
				setup: async (_component, info) => {
					const elementTypeKey = info.match.params.elementTypeKey;
					const groupKey = info.match.params.groupKey === 'null' ? null : info.match.params.groupKey;
					this.#workspaceContext!.create(elementTypeKey, groupKey);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this.#workspaceContext!,
						this.shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
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

	static styles = [UmbTextStyles];
}

export default UmbBlockTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-type-workspace': UmbBlockTypeWorkspaceElement;
	}
}
