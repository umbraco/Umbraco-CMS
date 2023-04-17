import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbUserWorkspaceContext } from './user-workspace.context';
import { UmbUserWorkspaceEditElement } from './user-workspace-edit.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { IRoute } from '@umbraco-cms/backoffice/router';

import '../../../shared/components/input-user-group/input-user-group.element';
import '../../../shared/property-editors/uis/document-picker/property-editor-ui-document-picker.element';
import '../../../shared/components/workspace/workspace-layout/workspace-layout.element';

@customElement('umb-user-workspace')
export class UmbUserWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles];

	#workspaceContext = new UmbUserWorkspaceContext(this);
	#element = new UmbUserWorkspaceEditElement();

	@state()
	_routes: IRoute[] = [
		{
			path: 'edit/:id',
			component: () => this.#element,
			setup: (component, info) => {
				const id = info.match.params.id;
				this.#workspaceContext.load(id);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}
}

export default UmbUserWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace': UmbUserWorkspaceElement;
	}
}
