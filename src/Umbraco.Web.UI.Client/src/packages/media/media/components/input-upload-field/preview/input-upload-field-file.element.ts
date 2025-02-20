import { html, customElement, property, state, css, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-input-upload-field-file')
export default class UmbInputUploadFieldFileElement extends UmbLitElement {
	#loadingText = `(${this.localize.term('general_loading')}...)`;

	#serverUrl = '';

	@property()
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

	constructor() {
		super();

		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();
		});
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('file') && this.file) {
			this.extension = this.file.name.split('.').pop() ?? '';
			this.label = this.file.name || this.#loadingText;
		}

		if (_changedProperties.has('path') && !this.file) {
			this.extension = this.path.split('.').pop() ?? '';
			this.label = this.path.split('/').pop() ?? this.#loadingText;
		}
	}

	override render() {
		if (!this.label && !this.extension) return html`<uui-loader></uui-loader>`;

		return html`
			<div id="main">
				<uui-symbol-file id="file-symbol" .type=${this.extension}></uui-symbol-file>
				${when(
					!this.file && this.path,
					() => html`<a id="label" href="${this.#serverUrl}${this.path}" target="_blank">${this.label}</a>`,
					() => html`<span id="label">${this.label}</span>`,
				)}
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

				&:is(a) {
					text-decoration: none;
					color: var(--uui-color-interactive);

					&:hover {
						text-decoration: underline;
						color: var(--uui-color-interactive-emphasis);
					}
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field-file': UmbInputUploadFieldFileElement;
	}
}
