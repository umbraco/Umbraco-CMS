import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-svg')
export default class UmbInputUploadFieldSvgElement extends UmbLitElement {
	@property({ type: String })
	path = '';

	get #label() {
		return this.path.split('/').pop() ?? '';
	}

	override render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;

		return html`<uui-card-media id="card" .name=${this.#label} href="${this.path}" target="_blank">
			<img id="image" src=${this.path} alt="svg" />
		</uui-card-media>`;
	}

	static override readonly styles = [
		css`
			:host {
				position: relative;
				min-height: 240px;
				max-height: 400px;
				width: fit-content;
				max-width: 100%;
			}

			#card {
				width: 100%;
				height: 100%;
			}

			#image {
				object-fit: contain;
				width: auto;
				height: 100%;
				background-color: #fff;
				background-image: url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>');
				background-repeat: repeat;
				background-size: 10px 10px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-svg': UmbInputUploadFieldSvgElement;
	}
}
