import type { UmbFileUploadPreviewElement } from '../file-upload-preview.interface.js';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-video')
export default class UmbInputUploadFieldVideoElement extends UmbLitElement implements UmbFileUploadPreviewElement {
	@property({ type: String })
	path = '';

	override render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;
		const label = this.path.split('/').pop() ?? '';
		return html`
			<video controls title=${label}>
				<source src=${this.path} />
				Video format not supported
			</video>
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-video': UmbInputUploadFieldVideoElement;
	}
}
