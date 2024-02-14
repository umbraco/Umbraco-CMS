import { UmbTemplateWorkspaceContext } from './template-workspace.context.js';
import { UmbTemplateWorkspaceEditorElement } from './template-workspace-editor.element.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { IRoutingInfo, PageComponent, UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../../components/insert-menu/templating-insert-menu.element.js';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-template-workspace')
export class UmbTemplateWorkspaceElement extends UmbLitElement {
	#templateWorkspaceContext = new UmbTemplateWorkspaceContext(this);
	#createElement = () => new UmbTemplateWorkspaceEditorElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentId',
			component: this.#createElement,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
				this.#templateWorkspaceContext.create(parentId);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#templateWorkspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!,
				);
			},
		},
		{
			path: 'edit/:key',
			component: this.#createElement,
			setup: (component: PageComponent, info: IRoutingInfo): void => {
				const key = info.match.params.key;
				this.#templateWorkspaceContext.load(key);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbTemplateWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-workspace': UmbTemplateWorkspaceElement;
	}
}
