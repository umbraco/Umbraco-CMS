import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../shared/node/editor-node.element';

@customElement('umb-editor-content')
export class UmbEditorContentElement extends LitElement {
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
		return html`<umb-editor-node id=${this.id} alias="Umb.Editor.Content"></umb-editor-node>`;
	}
}

export default UmbEditorContentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-content': UmbEditorContentElement;
	}
}
