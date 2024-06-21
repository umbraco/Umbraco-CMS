import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-image')
export class UmbInputUploadFieldImageElement extends UmbLitElement {
	@property({ type: String })
	path = '';

	render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;

		return html`<img src=${this.path} alt="" />`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				height: 100%;
				position: relative;
				width: fit-content;
				max-height: 400px;
			}

			img {
				max-width: 100%;
				max-height: 100%;
				object-fit: contain;
				width: auto;
				height: auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-image': UmbInputUploadFieldImageElement;
	}
}
