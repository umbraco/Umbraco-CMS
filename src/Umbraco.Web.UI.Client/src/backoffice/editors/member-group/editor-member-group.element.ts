import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../shared/editor-entity/editor-entity.element';

@customElement('umb-editor-member-group')
export class UmbEditorMemberGroupElement extends LitElement {
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
		return html` <umb-editor-entity alias="Umb.Editor.MemberGroup">Member Group Editor</umb-editor-entity> `;
	}
}

export default UmbEditorMemberGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-member-group': UmbEditorMemberGroupElement;
	}
}
