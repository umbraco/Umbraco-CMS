import { UmbScriptsWorkspaceContext } from './scripts-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-scripts-workspace')
export class UmbScriptsWorkspaceElement extends UmbLitElement {
	#scriptsWorkspaceContext = new UmbScriptsWorkspaceContext(this);
	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentKey',
			component: import('./scripts-workspace-edit.element.js'),
			setup: async (component: PageComponent, info: IRoutingInfo) => {
				const parentKey = info.match.params.parentKey;
				const decodePath = decodeURIComponent(parentKey);
				this.#scriptsWorkspaceContext.create(decodePath);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#scriptsWorkspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!,
				);
			},
		},
		{
			path: 'edit/:key',
			component: import('./scripts-workspace-edit.element.js'),
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const key = info.match.params.key;
				const decodePath = decodeURIComponent(key).replace('-js', '.js');
				this.#scriptsWorkspaceContext.load(decodePath);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbScriptsWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-scripts-workspace': UmbScriptsWorkspaceElement;
	}
}
