import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-member-workspace-edit')
export class UmbMemberWorkspaceEditElement extends LitElement {
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

	render() {
		return html` <umb-workspace-layout alias="Umb.Workspace.Member">Member Workspace</umb-workspace-layout> `;
	}
}

export default UmbMemberWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-edit': UmbMemberWorkspaceEditElement;
	}
}
