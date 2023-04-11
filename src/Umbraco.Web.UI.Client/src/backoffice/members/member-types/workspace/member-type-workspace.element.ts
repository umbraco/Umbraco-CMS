import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbMemberTypeWorkspaceEditElement } from './member-type-workspace-edit.element';
import { UmbMemberTypeWorkspaceContext } from './member-type-workspace.context';
import { IRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-member-type-workspace')
export class UmbMemberTypeWorkspaceElement extends UmbLitElement {
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

	#workspaceContext = new UmbMemberTypeWorkspaceContext(this);
	#element = new UmbMemberTypeWorkspaceEditElement();

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

export default UmbMemberTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-workspace': UmbMemberTypeWorkspaceElement;
	}
}
