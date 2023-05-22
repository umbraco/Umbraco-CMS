import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-member-workspace-edit')
export class UmbMemberWorkspaceEditElement extends LitElement {
	render() {
		return html` <umb-workspace-editor alias="Umb.Workspace.Member">Member Workspace</umb-workspace-editor> `;
	}

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
}

export default UmbMemberWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-edit': UmbMemberWorkspaceEditElement;
	}
}
