import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-member-group-workspace')
export class UmbMemberGroupWorkspaceElement extends LitElement {
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
			<umb-workspace-entity alias="Umb.Workspace.MemberGroup">Member Group Workspace</umb-workspace-entity>
		`;
	}
}

export default UmbMemberGroupWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace': UmbMemberGroupWorkspaceElement;
	}
}
