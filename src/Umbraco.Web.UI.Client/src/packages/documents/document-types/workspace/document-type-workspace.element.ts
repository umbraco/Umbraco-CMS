import { UmbDocumentTypeWorkspaceContext } from './document-type-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { html , customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbRoute, UmbRouterSlotInitEvent, generateRoutePathBuilder } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-document-type-workspace')
export class UmbDocumentTypeWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbDocumentTypeWorkspaceContext(this);
	#routerPath? = '';

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentId',
			component: import('./document-type-workspace-editor.element.js'),
			setup: (_component, info) => {
				const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
				this.#workspaceContext.createScaffold(parentId);

				// Navigate to edit route when language is created:
				this.observe(
					this.#workspaceContext.isNew,
					(isNew) => {
						if (isNew === false) {
							const id = this.#workspaceContext.getEntityId();
							if (this.#routerPath && id) {
								const routeBasePath = this.#routerPath.endsWith('/') ? this.#routerPath : this.#routerPath + '/';
								// TODO: Revisit if this is the right way to change URL:
								const newPath = generateRoutePathBuilder(routeBasePath + 'edit/:id')({ id });
								window.history.pushState({}, '', newPath);
							}
						}
					},
					'_observeIsNew'
				);
			},
		},
		{
			path: 'edit/:id',
			component: import('./document-type-workspace-editor.element.js'),
			setup: (_component, info) => {
				const id = info.match.params.id;
				this.#workspaceContext.load(id);
			},
		},
	];

	render() {
		return html` <umb-router-slot .routes=${this._routes} @init=${(event: UmbRouterSlotInitEvent) => {
			this.#routerPath = event.target.absoluteRouterPath;
		}}></umb-router-slot> `;
	}

	static styles = [UUITextStyles];
}

export default UmbDocumentTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace': UmbDocumentTypeWorkspaceElement;
	}
}
