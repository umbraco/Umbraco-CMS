import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-member-group-edit-workspace
 * @description - Element for displaying a Member Group Workspace
 */
@customElement('umb-member-group-workspace-edit')
export class UmbMemberGroupWorkspaceEditElement extends UmbLitElement {
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
		return html` <umb-workspace-layout alias="Umb.Workspace.MemberGroup"> Member Group </umb-workspace-layout> `;
	}
}

export default UmbMemberGroupWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace-edit': UmbMemberGroupWorkspaceEditElement;
	}
}
