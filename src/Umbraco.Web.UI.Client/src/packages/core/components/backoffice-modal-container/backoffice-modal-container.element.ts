import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, CSSResultGroup, html, repeat, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalElement,
	UmbModalContext,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-backoffice-modal-container')
export class UmbBackofficeModalContainerElement extends UmbLitElement {
	@state()
	private _modalElementMap: Map<string, UmbModalElement> = new Map();

	@state()
	_modalHandlers: Array<UmbModalContext> = [];

	private _modalManagerContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalManagerContext = instance;
			this._observeModals();
		});
	}

	private _observeModals() {
		if (!this._modalManagerContext) return;
		this.observe(this._modalManagerContext.modals, (modalHandlers) => this.#createModalElements(modalHandlers));
	}

	/** We cannot render the umb-modal element directly in the uui-modal-container because it wont get reconised by UUI.
	 * We therefore have a helper class which creates the uui-modal element and returns it. */
	#createModalElements(modalHandlers: Array<UmbModalContext>) {
		this._modalHandlers = modalHandlers;

		if (this._modalHandlers.length === 0) {
			this._modalElementMap.clear();
			return;
		}

		this._modalHandlers.forEach((modalHandler) => {
			if (this._modalElementMap.has(modalHandler.key)) return;

			const modalElement = new UmbModalElement();
			modalElement.modalHandler = modalHandler;

			this._modalElementMap.set(modalHandler.key, modalElement);
		});
	}

	render() {
		return html`
			<uui-modal-container>
				${this._modalHandlers.length > 0
					? repeat(
							this._modalHandlers,
							(modalHandler) => html`${this._modalElementMap.get(modalHandler.key)?.modalElement}`,
					  )
					: ''}
			</uui-modal-container>
		`;
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
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
