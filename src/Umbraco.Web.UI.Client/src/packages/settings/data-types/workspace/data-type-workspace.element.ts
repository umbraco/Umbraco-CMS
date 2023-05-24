import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDataTypeWorkspaceContext } from './data-type-workspace.context.js';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './data-type-workspace-edit.element.js';

@customElement('umb-data-type-workspace')
export class UmbDataTypeWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbDataTypeWorkspaceContext(this);

	#element = document.createElement('umb-data-type-workspace-edit-element');

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentId',
			component: () => this.#element,
			setup: (_component, info) => {
				const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
				this.#workspaceContext.createScaffold(parentId);
			},
		},
		{
			path: 'edit/:id',
			component: () => this.#element,
			setup: (_component, info) => {
				const id = info.match.params.id;
				this.#workspaceContext.load(id);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbDataTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace': UmbDataTypeWorkspaceElement;
	}
}
