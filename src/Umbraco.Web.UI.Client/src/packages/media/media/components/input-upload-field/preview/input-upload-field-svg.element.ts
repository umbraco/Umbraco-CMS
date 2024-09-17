import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-svg')
export default class UmbInputUploadFieldSvgElement extends UmbLitElement {
	@property({ type: String })
	path = '';

	override render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;

		return html`<img src=${this.path} alt="svg" />`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				background-color: #fff;
				background-image: url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>');
				background-repeat: repeat;
				background-size: 10px 10px;
				height: 100%;
				min-height: 240px;
				position: relative;
				width: fit-content;
				max-height: 240px;
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
		'umb-input-upload-field-svg': UmbInputUploadFieldSvgElement;
	}
}
