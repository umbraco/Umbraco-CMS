import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-upload-field-file')
export default class UmbInputUploadFieldFileElement extends UmbLitElement {
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

	#loadingText = `(${this.localize.term('general_loading')}...)`;

	/**
	 *
	 */
	constructor() {
		super();
		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();
		}).asPromise();
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('file') && this.file) {
			this.extension = this.file.name.split('.').pop() ?? '';
			this.label = this.file.name || this.#loadingText;
		}

		if (_changedProperties.has('path')) {
			if (this.#serverUrl) {
				if (this.file) return;

				this.extension = this.path.split('.').pop() ?? '';
				this.label = this.#serverUrl ? this.path.substring(this.#serverUrl.length) : this.#loadingText;
			}
		}
	}

	#renderLabel() {
		if (this.path) {
			// Don't make it a link if it's a temp file upload.
			return this.file ? this.label : html`<a id="label" href=${this.path} target="_blank">${this.label}</a>`;
		}

		return html`<span id="label">${this.label}</span>`;
	}

	override render() {
		if (!this.label && !this.extension) return html`<uui-loader></uui-loader>`;

		return html`
			<div id="main">
				<uui-symbol-file id="file-symbol" .type=${this.extension}></uui-symbol-file>
				${this.#renderLabel()}
			</div>
		`;
	}

	static override readonly styles = [
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
			#label {
				text-align: center;
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
				color: var(--uui-color-text);
			}
			a#label {
				text-decoration: none;
				color: var(--uui-color-interactive);
			}
			a#label:hover {
				text-decoration: underline;
				color: var(--uui-color-interactive-emphasis);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-file': UmbInputUploadFieldFileElement;
	}
}
