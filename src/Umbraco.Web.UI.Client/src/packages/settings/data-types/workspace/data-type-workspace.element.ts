import { UmbDataTypeWorkspaceContext } from './data-type-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './data-type-workspace-editor.element.js';
import type { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-data-type-workspace')
export class UmbDataTypeWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbDataTypeWorkspaceContext(this);

	#element = document.createElement('umb-data-type-workspace-editor');

	@property({ type: Object, attribute: false })
	public set preset(value: Partial<DataTypeResponseModel>) {
		this.#workspaceContext.setPreset(value);
	}

	private _routes: UmbRoute[] = [
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

	static styles = [UUITextStyles];
}

export default UmbDataTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace': UmbDataTypeWorkspaceElement;
	}
}
