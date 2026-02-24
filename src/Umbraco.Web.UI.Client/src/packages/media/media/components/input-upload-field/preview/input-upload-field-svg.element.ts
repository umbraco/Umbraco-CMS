import type { UmbFileUploadPreviewElement } from '../file-upload-preview.interface.js';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-svg')
export default class UmbInputUploadFieldSvgElement extends UmbLitElement implements UmbFileUploadPreviewElement {
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
			<img src=${this.path} alt=${this.#fileName} loading="lazy" />
			<span id="filename" title=${this.#fileName}>${this.#fileName}</span>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				height: 100%;
				min-height: 240px;
				max-height: 400px;

				width: fit-content;
				min-width: 240px;
				max-width: 100%;
			}

			img {
				object-fit: contain;
				width: auto;
				height: 100%;
				background-color: #fff;
				background-image: url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>');
				background-repeat: repeat;
				background-size: 10px 10px;
				max-width: 100%;
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
		'umb-input-upload-field-svg': UmbInputUploadFieldSvgElement;
	}
}
