import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbMemberWorkspaceEditElement } from './member-workspace-edit.element';
import { UmbMemberWorkspaceContext } from './member-workspace.context';
import type { IRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-member-workspace')
export class UmbMemberWorkspaceElement extends UmbLitElement {
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

	#workspaceContext = new UmbMemberWorkspaceContext(this);
	#element = new UmbMemberWorkspaceEditElement();

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
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}
}

export default UmbMemberWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace': UmbMemberWorkspaceElement;
	}
}
