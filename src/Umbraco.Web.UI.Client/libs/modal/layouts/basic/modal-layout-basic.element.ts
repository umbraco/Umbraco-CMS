import { css, html, TemplateResult } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbBasicModalData {
    header: TemplateResult | string;
	content: TemplateResult | string;
    overlaySize?: UUIModalSidebarSize;
}

@customElement('umb-modal-layout-basic')
export class UmbModalLayoutBasicElement extends UmbModalLayoutElement<UmbBasicModalData> {
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
            <umb-workspace-layout .headline=${this.data?.header}>
				<uui-scroll-container>${this.data?.content}</uui-scroll-container>
				<uui-button slot="actions" look="secondary" label="Close sidebar" @click="${this._close}">Close</uui-button>
			</umb-workspace-layout>
		`;
	}	
}

export default UmbModalLayoutBasicElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-basic': UmbModalLayoutBasicElement;
	}
}
