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
	_modals: Array<UmbModalContext> = [];

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
		this.observe(this._modalManagerContext.modals, (modals) => this.#createModalElements(modals));
	}

	/** We cannot render the umb-modal element directly in the uui-modal-container because it wont get reconised by UUI.
	 * We therefore have a helper class which creates the uui-modal element and returns it. */
	#createModalElements(modals: Array<UmbModalContext>) {
		this._modals = modals;

		if (this._modals.length === 0) {
			this._modalElementMap.clear();
			return;
		}

		this._modals.forEach((modal) => {
			if (this._modalElementMap.has(modal.key)) return;

			const modalElement = new UmbModalElement();
			modalElement.modalContext = modal;

			// TODO: We need to change this to the close-end event, when it is added to UUI again.
			// This solution solves the memory leak issue where the modal contexts where not removed from the manager when they are closed.
			// It breaks the modal animation though, so we need to wait for the close-end so we are sure the animation is done.
			modalElement.element?.addEventListener('close', () => this._modalManagerContext?.remove(modal.key));

			this._modalElementMap.set(modal.key, modalElement);
		});
	}

	#renderModal(key: string) {
		const modalElement = this._modalElementMap.get(key);
		if (!modalElement) return;
		return html`${modalElement.render()}`;
	}

	render() {
		return html`
			<uui-modal-container>
				${this._modals.length > 0
					? repeat(
							this._modals,
							(modal) => modal.key,
							(modal) => this.#renderModal(modal.key),
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
