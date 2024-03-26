import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, repeat, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbModalManagerContext, UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalElement } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-backoffice-modal-container')
export class UmbBackofficeModalContainerElement extends UmbLitElement {
	@state()
	private _modalElementMap: Map<string, UmbModalElement> = new Map();

	@state()
	_modals: Array<UmbModalContext> = [];

	private _modalManager?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			//TODO: This is being called way to many times if first page load includes an open modal.
			this._modalManager = instance;
			this._observeModals();
		});
	}

	private _observeModals() {
		if (!this._modalManager) return;
		this.observe(this._modalManager.modals, (modals) => {
			this.#createModalElements(modals);
		});
	}

	/** We cannot render the umb-modal element directly in the uui-modal-container because it wont get recognized by UUI.
	 *  We therefore have a helper class which creates the uui-modal element and returns it. */
	#createModalElements(modals: Array<UmbModalContext>) {
		const oldValue = this._modals;
		this._modals = modals;

		const oldModals = oldValue.filter((oldModal) => !modals.some((modal) => modal.key === oldModal.key));

		oldModals.forEach((modal) => {
			// TODO: I would not think this works as expected, the callback method has to be the exact same instance as the one added: [NL]
			this._modalElementMap.get(modal.key)?.removeEventListener('close-end', this.#onCloseEnd.bind(this, modal.key));
			this._modalElementMap.delete(modal.key);
		});

		if (this._modals.length === 0) {
			//this._modalElementMap.clear(); // should not make a difference now that we clean it above. [NL]
			return;
		}

		this._modals.forEach((modal) => {
			if (this._modalElementMap.has(modal.key)) return;

			const modalElement = new UmbModalElement();
			modalElement.modalContext = modal;

			modalElement.element?.addEventListener('close-end', this.#onCloseEnd.bind(this, modal.key));
			modal.addEventListener('umb:destroy', this.#onCloseEnd.bind(this, modal.key));

			this._modalElementMap.set(modal.key, modalElement);
			this.requestUpdate();
		});
	}

	#onCloseEnd(key: string) {
		this._modalManager?.remove(key);
	}

	#renderModal(key: string) {
		const modalElement = this._modalElementMap.get(key);
		if (!modalElement) return nothing;

		return modalElement.render();
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
