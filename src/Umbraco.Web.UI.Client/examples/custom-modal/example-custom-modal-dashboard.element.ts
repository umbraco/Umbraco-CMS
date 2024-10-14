import { EXAMPLE_MODAL_TOKEN, type ExampleModalData, type ExampleModalResult } from './example-modal-token.js';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {  UMB_MODAL_MANAGER_CONTEXT, type UmbModalContext } from '@umbraco-cms/backoffice/modal';
import './example-custom-modal-element.element.js';

@customElement('example-custom-modal-dashboard')
export class UmbExampleCustomModalDashboardElement extends UmbLitElement {

	#modalManagerContext? : typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	/**
	 *
	 */
	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT,(instance)=>{
			this.#modalManagerContext = instance;
		})
	}

	#onOpenModal(){
		this.#modalManagerContext?.open(this,EXAMPLE_MODAL_TOKEN,{})
	}

	override render() {
		return html`
			<uui-box>
				<p>Open the custom modal</p>
				<uui-button @click=${this.#onOpenModal}>Open Modal</uui-button>
			</uui-box>
		`;
	}

	static override styles = [css`


	`];
}

export default UmbExampleCustomModalDashboardElement

declare global {
    interface HTMLElementTagNameMap {
        'example-custom-modal-dashboard': UmbExampleCustomModalDashboardElement;
    }
}
