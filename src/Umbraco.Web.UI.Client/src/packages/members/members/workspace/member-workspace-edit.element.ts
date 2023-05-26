import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, LitElement , customElement } from '@umbraco-cms/backoffice/external/lit';

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
