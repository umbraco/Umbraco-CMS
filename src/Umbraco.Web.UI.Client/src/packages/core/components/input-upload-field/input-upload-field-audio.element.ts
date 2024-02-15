import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-audio')
export class UmbInputUploadFieldAudioElement extends UmbLitElement {
	@property({ type: String })
	path = '';

	render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;

		return html`<audio controls src=${this.path}></audio>`;
	}

	static styles = [
		css`
			:host {
				display: flex;
				flex-grow: 1;
			}
			audio {
				width: 100%;
				max-width: 600px;
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-audio': UmbInputUploadFieldAudioElement;
	}
}
