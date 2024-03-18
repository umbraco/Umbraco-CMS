import type { UmbDocumentBlueprintWorkspaceContext } from './document-blueprint-workspace.context.js';
import { UmbDocumentBlueprintWorkspaceEditorElement } from './document-blueprint-workspace-editor.element.js';
import { UmbDocumentBlueprintRootWorkspaceElement } from './document-blueprint-root-workspace.element.js';
import { html, customElement, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { type UmbApi, createExtensionApi, UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry, type ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-document-blueprint-workspace')
export class UmbDocumentBlueprintWorkspaceElement extends UmbLitElement {
	#workspaceContext?: UmbDocumentBlueprintWorkspaceContext;
	#editorElement = () => new UmbDocumentBlueprintWorkspaceEditorElement();
	#rootElement = () => new UmbDocumentBlueprintRootWorkspaceElement();

	@state()
	_routes: UmbRoute[] = [];

	public set manifest(manifest: ManifestWorkspace) {
		createExtensionApi(this, manifest).then((context) => {
			if (context) {
				this.#gotWorkspaceContext(context);
			}
		});
	}

	#gotWorkspaceContext(context: UmbApi) {
		this.#workspaceContext = context as UmbDocumentBlueprintWorkspaceContext;

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
				/** TODO: Any way to setup the root without edit/null appearing? */
				path: 'edit/null',
				component: this.#rootElement,
			},
			{
				path: 'edit/:unique',
				component: this.#editorElement,
				setup: (_component, info) => {
					console.log('edit unique');
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

	static styles = [
		css`
			#wrapper {
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbDocumentBlueprintWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-workspace': UmbDocumentBlueprintWorkspaceElement;
	}
}
