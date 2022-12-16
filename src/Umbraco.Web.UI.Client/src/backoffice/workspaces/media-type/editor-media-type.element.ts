import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import '../shared/editor-entity-layout/editor-entity-layout.element';

@customElement('umb-editor-media-type')
export class UmbEditorMediaTypeElement extends LitElement {
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
		return html` <umb-workspace-entity-layout alias="Umb.Editor.MediaType">Media Type Editor</umb-workspace-entity-layout> `;
	}
}

export default UmbEditorMediaTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-media-type': UmbEditorMediaTypeElement;
	}
}
