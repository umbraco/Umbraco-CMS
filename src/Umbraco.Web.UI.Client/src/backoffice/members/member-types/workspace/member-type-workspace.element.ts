import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UmbMemberTypeWorkspaceEditElement } from './member-type-workspace-edit.element';
import { UmbMemberTypeWorkspaceContext } from './member-type-workspace.context';
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
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}
}

export default UmbMemberTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-workspace': UmbMemberTypeWorkspaceElement;
	}
}
