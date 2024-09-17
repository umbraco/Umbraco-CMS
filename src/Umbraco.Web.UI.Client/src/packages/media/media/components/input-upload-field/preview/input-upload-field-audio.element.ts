import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-audio')
export default class UmbInputUploadFieldAudioElement extends UmbLitElement {
	@property({ type: String })
	path = '';

	override render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;

		return html`<audio controls src=${this.path}></audio>`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				width: 999px;
				max-width: 100%;
			}
			audio {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-audio': UmbInputUploadFieldAudioElement;
	}
}
