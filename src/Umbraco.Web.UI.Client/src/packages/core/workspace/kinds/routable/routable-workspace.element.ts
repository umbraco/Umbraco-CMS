import type { UmbRoutableWorkspaceContext } from '../../contexts/tokens/routable-workspace-context.interface.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-routable-workspace')
export class UmbRoutableWorkspaceElement extends UmbLitElement {
	@state()
	_routes: UmbRoute[] = [];

	public set api(api: UmbRoutableWorkspaceContext) {
		this.observe(api.routes.routes, (routes) => (this._routes = routes));

		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [api]);
	}

	override render() {
		return html`<umb-router-slot .routes="${this._routes}"></umb-router-slot>`;
	}
}

export default UmbRoutableWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-routable-workspace': UmbRoutableWorkspaceElement;
	}
}
