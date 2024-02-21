import { UmbScriptWorkspaceContext } from './script-workspace.context.js';
import { UmbScriptWorkspaceEditorElement } from './script-workspace-editor.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-script-workspace')
export class UmbScriptWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbScriptWorkspaceContext(this);
	#createElement = () => new UmbScriptWorkspaceEditorElement();

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
