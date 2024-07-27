import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

@customElement('umb-example-modal-step2')
export class UmbExampleModalStep2 extends UmbModalBaseElement {

	override render() {
		return html`
			<div>
				example modal step1


			</div>
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbExampleModalStep2;

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-modal-step2': UmbExampleModalStep2;
	}
}
