import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

/**
 *  @element umb-workspace-property-layout
 *  @description - Element for displaying a property in an workspace.
 *  @slot editor - Slot for rendering the Property Editor
 *  @slot property-action-menu - Slot for rendering the Property Action Menu
 */
@customElement('umb-workspace-property-layout')
export class UmbWorkspacePropertyLayoutElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 200px 600px;
				gap: var(--uui-size-layout-2);
				border-bottom: 1px solid var(--uui-color-divider);
				padding: var(--uui-size-space-6) 0;
			}

			:host([orientation="vertical"]) {
				display:block;
			}

			:host(:last-of-type) {
				border-bottom:none;
			}

			p {
				margin-bottom: 0;
			}
			#header {
				position: sticky;
				top: var(--uui-size-space-4);
				height: min-content;
			}
		`,
	];

	/**
	 * Label. Name of the property.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public label = '';

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
		return html`
			<div id="header">
				<uui-label>${this.label}</uui-label>
				<slot name="property-action-menu"></slot>
				<p>${this.description}</p>
			</div>
			<div>
				<uui-form-validation-message>
					<slot name="editor"></slot>
				</uui-form-validation-message>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-property-layout': UmbWorkspacePropertyLayoutElement;
	}
}
