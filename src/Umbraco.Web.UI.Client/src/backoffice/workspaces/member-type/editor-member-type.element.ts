import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../shared/editor-entity-layout/editor-entity-layout.element';

@customElement('umb-editor-member-type')
export class UmbEditorMemberTypeElement extends LitElement {
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
			<umb-workspace-entity-layout alias="Umb.Editor.MemberType">Member Type Editor</umb-workspace-entity-layout>
		`;
	}
}

export default UmbEditorMemberTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-member-type': UmbEditorMemberTypeElement;
	}
}
