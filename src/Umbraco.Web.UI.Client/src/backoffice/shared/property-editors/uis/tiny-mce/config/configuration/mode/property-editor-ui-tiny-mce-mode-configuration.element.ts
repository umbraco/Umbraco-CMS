import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UUISelectEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tiny-mce-mode-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-mode-configuration')
export class UmbPropertyEditorUITinyMceModeConfigurationElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display:block;
				width:220px;
			}
			
			ul {
				list-style: none;
				padding: 0;
				margin: 0;
			}

			uui-select {
				max-width:220px;
			}
		`,
	];

	@property({ type: String })
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	options: Array<Option> = [
		{
			value: 'classic',
			name: 'Classic',
		},
		{
			value: 'inline',
			name: 'Inline',
		},
	];

	connectedCallback(): void {
		super.connectedCallback();

		this.options.forEach((o) => {
			o.selected = this.value === o.value;
		});
	}

	#onChange(event: UUISelectEvent) {
		this.value = event.target.value.toString();
	}

	// TODO => workspace-property should link the label to the input to resolve A11Y failures 
	render() {
		return html`<uui-select .options=${this.options} @change=${this.#onChange}></uui-select>`;
	}
}

export default UmbPropertyEditorUITinyMceModeConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-mode-configuration': UmbPropertyEditorUITinyMceModeConfigurationElement;
	}
}
