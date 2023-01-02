import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../../../core/components/workspace/workspace-entity/workspace-entity.element';

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
		return html` <umb-workspace-entity alias="Umb.Workspace.MemberType">Member Type Workspace</umb-workspace-entity> `;
	}
}

export default UmbMemberTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-workspace': UmbMemberTypeWorkspaceElement;
	}
}
