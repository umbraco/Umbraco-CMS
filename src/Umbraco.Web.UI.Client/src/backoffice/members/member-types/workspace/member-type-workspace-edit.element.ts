import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-member-type-workspace-edit')
export class UmbMemberTypeWorkspaceEditElement extends UmbLitElement {


	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.MemberType"> Member Type Workspace </umb-workspace-editor>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				/* TODO: can this be applied from layout slot CSS? */
				margin: 0 var(--uui-size-layout-1);
				flex: 1 1 auto;
			}
		`,
	];
}

export default UmbMemberTypeWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-workspace-edit': UmbMemberTypeWorkspaceEditElement;
	}
}
