import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { html, until, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

type FileItem = {
	name: string;
	src: string;
};

@customElement('umb-input-upload-field-audio')
export class UmbInputUploadFieldAudioElement extends UmbLitElement {
	@property({ type: String })
	path = '';

	#serverUrl = '';

	constructor() {
		super();
		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();
		});
	}

	render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;

		return html`<audio controls src=${this.#serverUrl + this.path}></audio>`;
	}

	static styles = [
		css`
			audio {
				width: 100%;
				max-width: 600px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-audio': UmbInputUploadFieldAudioElement;
	}
}
