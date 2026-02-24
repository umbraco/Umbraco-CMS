import type { UmbFileUploadPreviewElement } from '../file-upload-preview.interface.js';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-audio')
export default class UmbInputUploadFieldAudioElement extends UmbLitElement implements UmbFileUploadPreviewElement {
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
			<audio controls src=${this.path} title=${this.#fileName}></audio>
			<span id="filename" title=${this.#fileName}>${this.#fileName}</span>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				width: 999px;
				max-width: 100%;
			}

			audio {
				width: 100%;
			}

			#filename {
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
		'umb-input-upload-field-audio': UmbInputUploadFieldAudioElement;
	}
}
