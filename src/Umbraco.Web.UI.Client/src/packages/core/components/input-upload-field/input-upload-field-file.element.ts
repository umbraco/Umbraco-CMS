import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-file')
export class UmbInputUploadFieldFileElement extends UmbLitElement {
	@property({ type: String })
	path: string = '';

	/**
	 * @description The file to be rendered.
	 * @type {File}
	 * @required
	 */
	@property({ attribute: false })
	file?: File;

	@state()
	extension = '';

	@state()
	label = '';

	#serverUrl = '';
	#serverUrlPromise;

	/**
	 *
	 */
	constructor() {
		super();
		this.#serverUrlPromise = this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();
		}).asPromise();
	}

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('file')) {
			this.extension = this.file?.name.split('.').pop() || '';
			this.label = this.file?.name || 'loading...';
		}

		if (_changedProperties.has('path')) {
			if (this.#serverUrl) {
				this.extension = this.path.split('.').pop() || '';
				this.label = this.#serverUrl ? this.path.substring(this.#serverUrl.length) : 'loading...';
			}
		}
	}

	#renderLabel() {
		if (this.path) return html`<a href=${this.path}>${this.label}</a>`;

		return html`<span>${this.label}</span>`;
	}

	render() {
		if (!this.label && !this.extension) return html`<uui-loader></uui-loader>`;

		return html`
			<div id="main">
				<uui-symbol-file id="file-symbol" .type=${this.extension}></uui-symbol-file>
				${this.#renderLabel()}
			</div>
		`;
	}

	static styles = [
		css`
			#main {
				display: grid;
				grid-template-rows: 150px auto;
				box-sizing: border-box;
				color: var(--uui-color-text);
			}
			#file-symbol {
				aspect-ratio: 1 / 1;
				margin: auto;
				max-width: 100%;
				max-height: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-file': UmbInputUploadFieldFileElement;
	}
}
