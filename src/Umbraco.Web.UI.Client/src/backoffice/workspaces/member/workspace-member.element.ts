import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-workspace-member')
export class UmbWorkspaceMemberElement extends LitElement {
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
		return html` <umb-workspace-entity-layout alias="Umb.Workspace.Member">Member Workspace</umb-workspace-entity-layout> `;
	}
}

export default UmbWorkspaceMemberElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-member': UmbWorkspaceMemberElement;
	}
}
