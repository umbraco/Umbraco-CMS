import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-video')
export class UmbInputUploadFieldVideoElement extends UmbLitElement {
	@property({ type: String })
	path = '';

	render() {
		if (!this.path) return html`<uui-loader></uui-loader>`;

		return html`
			<video controls>
				<source src=${this.path} />
				Video format not supported
			</video>
		`;
	}

	static styles = [
		css`
			:host {
				display: flex;
			}
			video {
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
