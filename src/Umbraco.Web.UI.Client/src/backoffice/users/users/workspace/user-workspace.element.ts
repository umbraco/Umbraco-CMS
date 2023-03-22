import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbUserWorkspaceContext } from './user-workspace.context';
import { UmbUserWorkspaceEditElement } from './user-workspace-edit.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { IRoutingInfo } from '@umbraco-cms/internal/router';

import '../../../shared/components/input-user-group/input-user-group.element';
import '../../../shared/property-editors/uis/document-picker/property-editor-ui-document-picker.element';
import '../../../shared/components/workspace/workspace-layout/workspace-layout.element';

@customElement('umb-user-workspace')
export class UmbUserWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles];

	#workspaceContext = new UmbUserWorkspaceContext(this);
	#element = new UmbUserWorkspaceEditElement();

	@state()
	_routes: any[] = [
		{
			path: 'edit/:key',
			component: () => this.#element,
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const key = info.match.params.key;
				this.#workspaceContext.load(key);
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
