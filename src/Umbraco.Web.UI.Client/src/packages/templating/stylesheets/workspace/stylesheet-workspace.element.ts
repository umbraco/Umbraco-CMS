import { UmbStylesheetWorkspaceContext } from './stylesheet-workspace.context.js';
import { UmbStylesheetWorkspaceEditorElement } from './stylesheet-workspace-editor.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { IRoutingInfo, PageComponent, UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-stylesheet-workspace')
export class UmbStylesheetWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbStylesheetWorkspaceContext(this);
	#createElement = () => new UmbStylesheetWorkspaceEditorElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentUnique',
			component: this.#createElement,
			setup: async (component: PageComponent, info: IRoutingInfo) => {
				const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
				await this.#workspaceContext.create(parentUnique);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#workspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!,
				);
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

	render() {
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbStylesheetWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace': UmbStylesheetWorkspaceElement;
	}
}
