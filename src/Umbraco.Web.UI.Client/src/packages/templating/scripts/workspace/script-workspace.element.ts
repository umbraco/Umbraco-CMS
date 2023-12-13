import { UmbScriptWorkspaceContext } from './script-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-script-workspace')
export class UmbScriptWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbScriptWorkspaceContext(this);
	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentUnique',
			component: import('./script-workspace-edit.element.js'),
			setup: async (_component: PageComponent, info: IRoutingInfo) => {
				const parentUnique = info.match.params.parentUnique;
				const decodePath = decodeURIComponent(parentUnique);
				this.#workspaceContext.create(decodePath === 'null' ? '' : decodePath);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#workspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!,
				);
			},
		},
		{
			path: 'edit/:unique',
			component: import('./script-workspace-edit.element.js'),
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const unique = info.match.params.unique;
				const decodePath = decodeURIComponent(unique).replace('-js', '.js');
				this.#workspaceContext.load(decodePath);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbScriptWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-script-workspace': UmbScriptWorkspaceElement;
	}
}
