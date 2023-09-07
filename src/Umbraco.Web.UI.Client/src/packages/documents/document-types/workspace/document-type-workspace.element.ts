import { UmbDocumentTypeWorkspaceContext } from './document-type-workspace.context.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-document-type-workspace')
export class UmbDocumentTypeWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbDocumentTypeWorkspaceContext(this);

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentId',
			component: import('./document-type-workspace-editor.element.js'),
			setup: (_component, info) => {
				const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
				this.#workspaceContext.create(parentId);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#workspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!
				);
			},
		},
		{
			path: 'edit/:id',
			component: import('./document-type-workspace-editor.element.js'),
			setup: (_component, info) => {
				this.removeControllerByAlias('_observeIsNew');
				const id = info.match.params.id;
				this.#workspaceContext.load(id);
			},
		},
	];

	render() {
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}

	static styles = [UmbTextStyles];
}

export default UmbDocumentTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace': UmbDocumentTypeWorkspaceElement;
	}
}
