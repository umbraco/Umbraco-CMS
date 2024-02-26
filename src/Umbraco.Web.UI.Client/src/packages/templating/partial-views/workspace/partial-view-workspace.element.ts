import { UmbPartialViewWorkspaceContext } from './partial-view-workspace.context.js';
import { UmbPartialViewWorkspaceEditorElement } from './partial-view-workspace-editor.element.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

import '../../components/templating-item-menu/templating-item-menu.element.js';

@customElement('umb-partial-view-workspace')
export class UmbPartialViewWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbPartialViewWorkspaceContext(this);

	#createElement = () => new UmbPartialViewWorkspaceEditorElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentUnique/snippet/:snippetId',
			component: this.#createElement,
			setup: async (component: PageComponent, info: IRoutingInfo) => {
				const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
				const snippetId = info.match.params.snippetId;
				await this.#onCreate(parentUnique, snippetId);
			},
		},
		{
			path: 'create/:parentUnique',
			component: this.#createElement,
			setup: async (component: PageComponent, info: IRoutingInfo) => {
				const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
				await this.#onCreate(parentUnique);
			},
		},
		{
			path: 'edit/:unique',
			component: this.#createElement,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const unique = info.match.params.unique;
				this.#workspaceContext.load(unique);
			},
		},
	];

	#onCreate = async (parentUnique: string | null, snippetId?: string) => {
		await this.#workspaceContext.create(parentUnique, snippetId);

		new UmbWorkspaceIsNewRedirectController(
			this,
			this.#workspaceContext,
			this.shadowRoot!.querySelector('umb-router-slot')!,
		);
	};

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbPartialViewWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-workspace': UmbPartialViewWorkspaceElement;
	}
}
