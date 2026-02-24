import type { UmbFileUploadPreviewElement } from '../file-upload-preview.interface.js';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-file')
export default class UmbInputUploadFieldFileElement extends UmbLitElement implements UmbFileUploadPreviewElement {
	@property()
	path: string = '';

	@property({ attribute: false })
	file?: File;

	get #fileName(): string {
		if (this.file?.name) return this.file.name;
		return this.path.split('/').pop() ?? '';
	}

	override render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;
		const fileExt = this.path.split('.').pop() ?? '';
		return html`
			<uui-symbol-file .type=${fileExt}></uui-symbol-file>
			<span id="filename" title=${this.#fileName}>${this.#fileName}</span>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-3);
			}

			uui-symbol-file {
				flex-shrink: 0;
			}

			#filename {
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-file': UmbInputUploadFieldFileElement;
	}
}
