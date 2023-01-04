import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbWorkspacePropertyContext } from 'src/backoffice/shared/components/entity-property/workspace-property.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-property-editor-ui-textarea')
export class UmbPropertyEditorUITextareaElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-textarea {
				width: 100%;
			}
		`,
	];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	config = [];

	private propertyContext?: UmbWorkspacePropertyContext<string>;

	constructor() {
		super();

		this.consumeContext('umbPropertyContext', (instance) => {
			this.propertyContext = instance;
		});
	}

	private onInput(e: InputEvent) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-editor-change', { bubbles: true, composed: true }));
	}

	render() {
		return html`
			<uui-textarea .value=${this.value} @input=${this.onInput}></uui-textarea>
			${this.config?.map((property: any) => html`<div>${property.alias}: ${property.value}</div>`)}
			<button @click=${() => this.propertyContext?.resetValue()}>Reset</button>`;
	}
}

export default UmbPropertyEditorUITextareaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-textarea': UmbPropertyEditorUITextareaElement;
	}
}
