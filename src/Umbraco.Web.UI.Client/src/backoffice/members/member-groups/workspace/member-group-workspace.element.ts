import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceMemberGroupContext } from './member-group-workspace.context';
import { UmbMemberGroupWorkspaceEditElement } from './member-group-workspace-edit.element';
import type { IRoute } from '@umbraco-cms/backoffice/router';
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
	_routes: IRoute[] = [
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
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}
}

export default UmbMemberGroupWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace': UmbMemberGroupWorkspaceElement;
	}
}
