import { css, html, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { until } from 'lit/directives/until.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

type FileItem = {
	name: string;
	src: string;
};

@customElement('umb-input-upload-field-file')
export class UmbInputUploadFieldFileElement extends UmbLitElement {
	/**
	 * @description The file to be rendered.
	 * @type {File}
	 * @required
	 */
	@property({ type: File, attribute: false })
	set file(value: File) {
		this.#fileItem = new Promise((resolve) => {
			/**
			 * If the mimetype of the file is an image, we want to render a thumbnail of the image.
			 */
			if (value.type && /image\/.*/.test(value.type)) {
				const reader = new FileReader();

				reader.readAsDataURL(value);

				reader.onload = (event) => {
					resolve({
						name: value.name,
						src: event.target?.result as string,
					});
				};
			} else {
				resolve({
					name: value.name,
					src: '',
				});
			}
		});
	}

	#fileItem!: Promise<FileItem>;

	render = () => until(this.#renderFileItem(), html`<uui-loader></uui-loader>`);

	async #renderFileItem() {
		const fileItem = await this.#fileItem;
		return html`<uui-symbol-file-thumbnail
			src=${fileItem.src}
			title=${fileItem.name}
			alt=${fileItem.name}></uui-symbol-file-thumbnail>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-file': UmbInputUploadFieldFileElement;
	}
}
