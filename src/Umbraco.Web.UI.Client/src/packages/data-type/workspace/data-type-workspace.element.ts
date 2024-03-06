import { UmbDataTypeWorkspaceEditorElement } from './data-type-workspace-editor.element.js';
import { UmbDataTypeWorkspaceContext } from './data-type-workspace.context.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-data-type-workspace')
export class UmbDataTypeWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbDataTypeWorkspaceContext(this);
	#createElement = () => new UmbDataTypeWorkspaceEditorElement();

	private _routes: UmbRoute[] = [
		{
			path: 'create/parent/:entityType/:parentUnique',
			component: this.#createElement,
			setup: (_component, info) => {
				const parentEntityType = info.match.params.entityType;
				const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
				this.#workspaceContext.create({ entityType: parentEntityType, unique: parentUnique });

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
			setup: (_component, info) => {
				const unique = info.match.params.unique;
				this.#workspaceContext.load(unique);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbDataTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace': UmbDataTypeWorkspaceElement;
	}
}
