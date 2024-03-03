import type { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { UmbDocumentWorkspaceEditorElement } from './document-workspace-editor.element.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsApiInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-document-workspace')
export class UmbDocumentWorkspaceElement extends UmbLitElement {
	#workspaceContext?: UmbDocumentWorkspaceContext;
	#editorElement = () => new UmbDocumentWorkspaceEditorElement();

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
				path: 'create/parent/:entityType/:parentUnique/:documentTypeUnique',
				component: this.#editorElement,
				setup: async (_component, info) => {
					// TODO: Remember the perspective of permissions here, we need to check if the user has access to create a document of this type under this parent?
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const documentTypeUnique = info.match.params.documentTypeUnique;
					this.#workspaceContext!.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this.#workspaceContext!,
						this.shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: this.#editorElement,
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.#workspaceContext!.load(unique);
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

export default UmbDocumentWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace': UmbDocumentWorkspaceElement;
	}
}
