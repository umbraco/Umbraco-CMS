import { css, customElement, html, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '@umbraco-cms/backoffice/ufm';

/**
 *  @element umb-property-layout
 *  @description - Element for displaying a property in an workspace.
 *  @slot editor - Slot for rendering the Property Editor
 *  @slot description - Slot for rendering things below the label.
 *  @slot action-menu - Slot for rendering the Property Action Menu
 */
@customElement('umb-property-layout')
export class UmbPropertyLayoutElement extends UmbLitElement {
	/**
	 * Alias. The technical name of the property.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public alias = '';

	/**
	 * Label. Name of the property.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public label = '';

	/**
	 * Orientation: Horizontal is the default where label goes left and editor right.
	 * Vertical is where label goes above the editor.
	 * @enum ['horizontal', 'vertical']
	 * @attr
	 * @default ''
	 */
	@property({ type: String, reflect: true })
	public orientation: 'horizontal' | 'vertical' = 'horizontal';

	/**
	 * Description: render a description underneath the label.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public description = '';

	/**
	 * @description Make the property appear invalid.
	 * @type {boolean}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Boolean, reflect: true })
	public invalid?: boolean;

	/**
	 * @description Display a mandatory indicator.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	public mandatory?: boolean;

	override render() {
		// TODO: Only show alias on label if user has access to DocumentType within settings:
		return html`
			<div id="headerColumn">
				<uui-label id="label" title=${this.alias} ?required=${this.mandatory}>
					${this.localize.string(this.label)}
					${when(
						this.invalid,
						() => html`<div id="invalid-badge"><uui-badge color="invalid" attention>!</uui-badge></div>`,
					)}
				</uui-label>
				${this.#renderAlias()}
				<slot name="action-menu"></slot>
				${this.#renderDescription()}
				<slot name="description"></slot>
			</div>
			<div id="editorColumn">
				<umb-form-validation-message>
					<slot name="editor"></slot>
				</umb-form-validation-message>
			</div>
		`;
	}

	#renderAlias() {
		// Show alias below label when it's different from the label and not empty
		if (!this.alias || this.alias === this.label) return;
		return html`<div id="alias">${this.alias}</div>`;
	}

	#renderDescription() {
		if (!this.description) return;
		const ufmValue = { alias: this.alias, label: this.label, description: this.description };
		return html`<umb-ufm-render id="description" .markdown=${this.description} .value=${ufmValue}></umb-ufm-render>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 200px minmax(0, 1fr);
				column-gap: var(--uui-size-layout-2);
				padding: var(--uui-size-layout-1) 0;
				container-type: inline-size;
			}

			:host > div {
				grid-column: span 2;
			}

			@container (width > 700px) {
				:host(:not([orientation='vertical'])) > div {
					grid-column: span 1;
				}
			}

			#headerColumn {
				position: relative;
				height: min-content;
				top: var(--umb-property-layout-header-top);
			}
			@container (width > 700px) {
				:host(:not([orientation='vertical'])) #headerColumn {
					position: sticky;
					top: var(--umb-property-layout-header-top, calc(var(--uui-size-space-2) * -1));
				}
			}

			:host {
				/* TODO: Temp solution to not get a yellow asterisk when invalid. */
				--umb-temp-uui-color-invalid: var(--uui-color-invalid);
			}

			#label {
				position: relative;
				word-break: break-word;
				/* TODO: Temp solution to not get a yellow asterisk when invalid. */
				--uui-color-invalid: var(--uui-color-danger);
			}
			#invalid-badge {
				display: inline-block;
				position: relative;
				width: 18px;
				height: 1em;
				margin-right: 6px;
			}
			uui-badge {
				//height: var(--uui-color-invalid);
				background-color: var(--umb-temp-uui-color-invalid);
				color: var(--uui-color-invalid-contrast);
			}

			#alias {
				color: var(--uui-color-text-alt);
				font-size: var(--uui-font-size-3);
				margin-top: var(--uui-size-space-1);
			}

			#description {
				color: var(--uui-color-text-alt);
			}

			#editorColumn {
				margin-top: var(--uui-size-space-3);
			}
			@container (width > 700px) {
				:host(:not([orientation='vertical'])) #editorColumn {
					margin-top: 0;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-layout': UmbPropertyLayoutElement;
	}
}
