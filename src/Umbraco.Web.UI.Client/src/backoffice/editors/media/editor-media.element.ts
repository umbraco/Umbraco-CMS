import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../node/editor-node.element';

@customElement('umb-editor-media')
export class UmbEditorMediaElement extends LitElement {
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

	constructor() {
		super();
	}

	render() {
		return html`<umb-editor-node id=${this.id} alias="Umb.Editor.Media"></umb-editor-node>`;
	}
}

export default UmbEditorMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-media': UmbEditorMediaElement;
	}
}
