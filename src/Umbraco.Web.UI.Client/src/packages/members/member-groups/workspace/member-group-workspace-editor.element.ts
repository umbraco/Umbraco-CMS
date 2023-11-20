import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-member-group-workspace-editor')
export class UmbMemberGroupWorkspaceEditorElement extends LitElement {
	render() {
		return html` <umb-workspace-editor alias="Umb.Workspace.MemberGroup">MemberGroup Workspace</umb-workspace-editor> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbMemberGroupWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace-editor': UmbMemberGroupWorkspaceEditorElement;
	}
}
