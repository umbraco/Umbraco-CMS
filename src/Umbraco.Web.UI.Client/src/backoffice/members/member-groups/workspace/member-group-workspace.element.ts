import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UmbWorkspaceMemberGroupContext } from './member-group-workspace.context';
import { UmbMemberGroupWorkspaceEditElement } from './member-group-workspace-edit.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-member-group-workspace
 * @description - Element for displaying a Member Group Workspace
 */
@customElement('umb-member-group-workspace')
export class UmbMemberGroupWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];

	#workspaceContext = new UmbWorkspaceMemberGroupContext(this);
	#element = new UmbMemberGroupWorkspaceEditElement();

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

export default UmbMemberGroupWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace': UmbMemberGroupWorkspaceElement;
	}
}
