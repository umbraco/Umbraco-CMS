import type { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './document-workspace-editor.element.js';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';
import { UmbApi, UmbExtensionsApiInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { ManifestWorkspace, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-workspace')
export class UmbDocumentWorkspaceElement extends UmbLitElement {
	#workspaceContext?: UmbDocumentWorkspaceContext;

	@state()
	_routes: UmbRoute[] = [];

	public set manifest(manifest: ManifestWorkspace) {
		createExtensionApi(manifest, [this]).then((context) => {
			if (context) {
				this.#gotWorkspaceContext(context);
			}
		});
	}

	#gotWorkspaceContext(context: UmbApi) {
		this.#workspaceContext = context as UmbDocumentWorkspaceContext;

		this._routes = [
			{
				path: 'create/:parentId/:documentTypeKey',
				component: import('./document-workspace-editor.element.js'),
				setup: async (_component, info) => {
					// TODO: Remember the perspective of permissions here, we need to check if the user has access to create a document of this type under this parent?
					const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
					const documentTypeKey = info.match.params.documentTypeKey;
					this.#workspaceContext!.create(documentTypeKey, parentId);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this.#workspaceContext!,
						this.shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:id',
				component: import('./document-workspace-editor.element.js'),
				setup: (_component, info) => {
					const id = info.match.params.id;
					this.#workspaceContext!.load(id);
				},
			},
		];

		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [this, this.#workspaceContext]);
	}

	render() {
		return html`<umb-router-slot .routes="${this._routes}"></umb-router-slot>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbDocumentWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace': UmbDocumentWorkspaceElement;
	}
}
