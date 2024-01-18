import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import {
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';

@customElement('umb-dynamic-root-origin-picker-modal')
export class UmbDynamicRootOriginPickerModalModalElement extends UmbModalBaseElement {
	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Insert">
				<div id="main">
					<h3>DynamicRoot Origin Picker Modal</h3>
					<!-- TODO: List the Dynamic Root origins. [LK] -->
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" label="Close">Close</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbDynamicRootOriginPickerModalModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dynamic-root-origin-picker-modal': UmbDynamicRootOriginPickerModalModalElement;
	}
}
