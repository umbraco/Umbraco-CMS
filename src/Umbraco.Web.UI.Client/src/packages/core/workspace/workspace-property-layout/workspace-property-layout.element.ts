import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property } from 'lit/decorators.js';

/**
 *  @element umb-workspace-property-layout
 *  @description - Element for displaying a property in an workspace.
 *  @slot editor - Slot for rendering the Property Editor
 *  @slot description - Slot for rendering things below the label.
 *  @slot property-action-menu - Slot for rendering the Property Action Menu
 */
@customElement('umb-workspace-property-layout')
export class UmbWorkspacePropertyLayoutElement extends LitElement {
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
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public orientation: 'horizontal' | 'vertical' = 'horizontal';

	/**
	 * Description: render a description underneath the label.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public description = '';

	render() {
		// TODO: Only show alias on label if user has access to DocumentType within settings:
		return html`
			<div id="header">
				<uui-label title=${this.alias}>${this.label}</uui-label>
				<slot name="property-action-menu"></slot>
				<p>${this.description}</p>
				<slot name="description"></slot>
			</div>
			<div>
				<uui-form-validation-message>
					<slot name="editor"></slot>
				</uui-form-validation-message>
			</div>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 200px auto;
				column-gap: var(--uui-size-layout-2);
				border-bottom: 1px solid var(--uui-color-divider);
				padding: var(--uui-size-layout-1) 0;
				container-type: inline-size;
			}

			:host > div {
				grid-column: span 2;
			}

			@container (width > 600px) {
				:host(:not([orientation='vertical'])) > div {
					grid-column: span 1;
				}
			}

			:host(:last-of-type) {
				border-bottom: none;
			}

			:host-context(umb-variantable-property:first-of-type) {
				padding-top: 0;
			}

			p {
				margin-bottom: 0;
			}

			#header {
				position: sticky;
				top: var(--uui-size-space-4);
				height: min-content;
				z-index: 2;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-property-layout': UmbWorkspacePropertyLayoutElement;
	}
}
