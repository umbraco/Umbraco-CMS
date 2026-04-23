import type { UmbFileUploadPreviewElement } from '../file-upload-preview.interface.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-file')
export default class UmbInputUploadFieldFileElement extends UmbLitElement implements UmbFileUploadPreviewElement {
	@property()
	path: string = '';

	@property({ attribute: false })
	file?: File;

	override render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;
		const name = this.file?.name ?? this.path;
		const fileExt = name.split('.').pop() ?? '';
		return html`<uui-symbol-file .type=${fileExt}></uui-symbol-file>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-file': UmbInputUploadFieldFileElement;
	}
}
