import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../shared/node/editor-node.element';

@customElement('umb-editor-document')
export class UmbEditorDocumentElement extends LitElement {
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
	entityKey!: string;

	render() {
		return html`<umb-editor-node .entityKey=${this.entityKey} alias="Umb.Editor.Document"></umb-editor-node>`;
	}
}

export default UmbEditorDocumentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-document': UmbEditorDocumentElement;
	}
}
