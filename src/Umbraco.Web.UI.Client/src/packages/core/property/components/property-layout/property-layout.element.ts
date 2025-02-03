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
					${when(this.invalid, () => html`<uui-badge color="danger" attention>!</uui-badge>`)}
				</uui-label>
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
				border-bottom: 1px solid var(--uui-color-divider);
				padding: var(--uui-size-layout-1) 0;
			}

			:host(:last-of-type) {
				border-bottom: none;
			}

			:host > div {
				grid-column: span 2;
			}
			/*@container (width > 600px) {*/
			:host(:not([orientation='vertical'])) > div {
				grid-column: span 1;
			}
			/*}*/

			#headerColumn {
				position: relative;
				height: min-content;
			}
			/*@container (width > 600px) {*/
			:host(:not([orientation='vertical'])) #headerColumn {
				position: sticky;
				top: calc(var(--uui-size-space-2) * -1);
			}
			/*}*/

			#label {
				position: relative;
				word-break: break-word;
			}
			:host([invalid]) #label {
				color: var(--uui-color-danger);
			}
			uui-badge {
				right: -30px;
			}

			#description {
				color: var(--uui-color-text-alt);
			}

			#editorColumn {
				margin-top: var(--uui-size-space-3);
			}
			/*@container (width > 600px) {*/
			:host(:not([orientation='vertical'])) #editorColumn {
				margin-top: 0;
			}
			/*}*/
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-layout': UmbPropertyLayoutElement;
	}
}
