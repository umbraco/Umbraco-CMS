import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbModalLayoutElement } from '../../../../../libs/modal/layouts/modal-layout.element';
import { UmbBasicModalData } from '.';

@customElement('umb-basic-modal')
export class UmbBasicModalElement extends UmbModalLayoutElement<UmbBasicModalData> {
	static styles = [
		UUITextStyles,
		css`
			uui-scroll-container {
				background-color: var(--uui-color-surface);
			}
		`,
	];

	private _close() {
		// As this is a basic modal designed for viewing readonly info
		// Then we don't need to pass any data back to the parent when
		// we close/save the modal etc...
		this.modalHandler?.close();
	}

	connectedCallback(): void {
		super.connectedCallback();
	}

	render() {
		return html`
			<umb-workspace-layout headline=${ifDefined(this.data?.headline)}>
				<uui-scroll-container>${this.data?.content}</uui-scroll-container>
				<uui-button slot="actions" look="secondary" label="Close sidebar" @click="${this._close}">Close</uui-button>
			</umb-workspace-layout>
		`;
	}
}

export default UmbBasicModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-basic-modal': UmbBasicModalElement;
	}
}
