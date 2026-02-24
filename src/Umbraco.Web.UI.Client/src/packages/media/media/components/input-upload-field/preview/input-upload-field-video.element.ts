import type { UmbFileUploadPreviewElement } from '../file-upload-preview.interface.js';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-video')
export default class UmbInputUploadFieldVideoElement extends UmbLitElement implements UmbFileUploadPreviewElement {
	@property({ type: String })
	path = '';

	@property({ attribute: false })
	file?: File;

	get #fileName(): string {
		if (this.file?.name) return this.file.name;
		return this.path.split('/').pop() ?? '';
	}

	override render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;
		return html`
			<video controls title=${this.#fileName}>
				<source src=${this.path} />
				Video format not supported
			</video>
			<span id="filename" title=${this.#fileName}>${this.#fileName}</span>
		`;
	}

	static override readonly styles = [
		css`
			video {
				height: 100%;
				max-height: 500px;
				width: 100%;
				max-width: 800px;
			}

			#filename {
				display: block;
				margin-top: var(--uui-size-space-2);
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-video': UmbInputUploadFieldVideoElement;
	}
}
