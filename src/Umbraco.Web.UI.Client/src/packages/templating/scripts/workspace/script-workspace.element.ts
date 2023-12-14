import { UmbScriptWorkspaceContext } from './script-workspace.context.js';
import { UmbScriptWorkspaceEditElement } from './script-workspace-edit.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-script-workspace')
export class UmbScriptWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbScriptWorkspaceContext(this);
	#createElement = () => new UmbScriptWorkspaceEditElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentKey',
			component: this.#createElement,
			setup: async (_component: PageComponent, info: IRoutingInfo) => {
				const parentKey = info.match.params.parentKey;
				const decodePath = decodeURIComponent(parentKey);
				this.#workspaceContext.create(decodePath === 'null' ? '' : decodePath);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#workspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!,
				);
			},
		},
		{
			path: 'edit/:key',
			component: this.#createElement,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const key = info.match.params.key;
				const decodePath = decodeURIComponent(key).replace('-js', '.js');
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
