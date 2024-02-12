import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { html, until, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

type FileItem = {
	name: string;
	src: string;
};

@customElement('umb-input-upload-field-file')
export class UmbInputUploadFieldFileElement extends UmbLitElement {
	@property({ type: String })
	path = '';

	/**
	 * @description The file to be rendered.
	 * @type {File}
	 * @required
	 */
	@property({ attribute: false })
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
	#serverUrl = '';

	constructor() {
		super();
		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();
		});
	}

	// TODO Better way to do this....
	render = () => {
		if (this.path) {
			return html`<uui-symbol-file-thumbnail
				src=${this.#serverUrl + this.path}
				title=${this.path}
				alt=${this.path}></uui-symbol-file-thumbnail>`;
		} else {
			return until(this.#renderFileItem(), html`<uui-loader></uui-loader>`);
		}
	};

	// render = () => until(this.#renderFileItem(), html`<uui-loader></uui-loader>`);

	async #renderFileItem() {
		const fileItem = await this.#fileItem;
		return html`<uui-symbol-file-thumbnail
			src=${fileItem.src}
			title=${fileItem.name}
			alt=${fileItem.name}></uui-symbol-file-thumbnail> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-file': UmbInputUploadFieldFileElement;
	}
}
