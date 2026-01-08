import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-file')
export default class UmbInputUploadFieldFileElement extends UmbLitElement {
	@property()
	path: string = '';

	/**
	 * @description The file to be rendered.
	 * @type {File}
	 */
	@property({ attribute: false })
	file?: File;

	get #label() {
		if (this.file) {
			return this.file.name;
		}
		return this.path.split('/').pop() ?? `(${this.localize.term('general_loading')}...)`;
	}

	get #fileExtension() {
		return this.#label.split('.').pop() ?? '';
	}

	override render() {
		if (!this.path && !this.file) return html`<uui-loader></uui-loader>`;

		return html`
			<uui-card-media
				.name=${this.#label}
				.fileExt=${this.#fileExtension}
				href=${this.path}
				target="_blank"></uui-card-media>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-file': UmbInputUploadFieldFileElement;
	}
}
