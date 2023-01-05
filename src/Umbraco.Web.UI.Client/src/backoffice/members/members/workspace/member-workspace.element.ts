import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-member-workspace')
export class UmbMemberWorkspaceElement extends LitElement {
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
		return html` <umb-workspace-layout alias="Umb.Workspace.Member">Member Workspace</umb-workspace-layout> `;
	}
}

export default UmbMemberWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace': UmbMemberWorkspaceElement;
	}
}
