import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../../../shared/components/workspace/workspace-layout/workspace-layout.element';

@customElement('umb-member-type-workspace')
export class UmbMemberTypeWorkspaceElement extends LitElement {
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
		return html` <umb-workspace-layout alias="Umb.Workspace.MemberType">Member Type Workspace</umb-workspace-layout> `;
	}
}

export default UmbMemberTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-workspace': UmbMemberTypeWorkspaceElement;
	}
}
