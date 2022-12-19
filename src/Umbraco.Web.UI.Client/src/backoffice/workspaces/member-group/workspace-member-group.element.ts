import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-workspace-member-group')
export class UmbWorkspaceMemberGroupElement extends LitElement {
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

	@property()
	id!: string;

	render() {
		return html`
			<umb-workspace-entity-layout alias="Umb.Workspace.MemberGroup">Member Group Workspace</umb-workspace-entity-layout>
		`;
	}
}

export default UmbWorkspaceMemberGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-member-group': UmbWorkspaceMemberGroupElement;
	}
}
