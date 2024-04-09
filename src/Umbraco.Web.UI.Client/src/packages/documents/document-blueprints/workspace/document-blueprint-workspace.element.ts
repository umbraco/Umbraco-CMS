import { UmbDocumentBlueprintWorkspaceContext } from './document-blueprint-workspace.context.js';
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
	#workspaceContext = new UmbDocumentBlueprintWorkspaceContext(this);
	#editorElement = () => new UmbDocumentBlueprintWorkspaceEditorElement();
	#rootElement = () => new UmbDocumentBlueprintRootWorkspaceElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/parent/:entityType/:parentUnique/:documentTypeUnique',
			component: this.#editorElement,
			setup: (_component, info) => {
				const parentEntityType = info.match.params.entityType;
				const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
				const documentTypeUnique = info.match.params.documentTypeUnique;
				this.#workspaceContext.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#workspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!,
				);
			},
		},
		{
			path: 'edit/:unique',
			component: this.#editorElement,
			setup: (_component, info) => {
				this.removeUmbControllerByAlias('isNewRedirectController');
				const unique = info.match.params.unique;
				this.#workspaceContext.load(unique);
			},
		},
	];

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
