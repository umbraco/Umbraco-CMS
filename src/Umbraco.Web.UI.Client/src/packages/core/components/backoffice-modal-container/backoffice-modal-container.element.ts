import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, CSSResultGroup, html, repeat, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbModalContext,
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-backoffice-modal-container')
export class UmbBackofficeModalContainerElement extends UmbLitElement {
	@state()
	private _modals?: UmbModalContext[];

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
			this._observeModals();
		});
	}

	private _observeModals() {
		if (!this._modalContext) return;

		this.observe(this._modalContext.modals, (modals) => {
			this._modals = modals;
		});
	}

	render() {
		return html`
			<uui-modal-container>
				${this._modals ? repeat(this._modals, (modalContext) => html`${modalContext.modalElement}`) : ''}
			</uui-modal-container>
		`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				position: absolute;
				z-index: 1000;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-modal-container': UmbBackofficeModalContainerElement;
	}
}
