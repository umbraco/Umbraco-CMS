import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UUIModalDialogElement } from '@umbraco-cms/backoffice/external/uui';

/**
 * A modal dialog element that cannot be dismissed by pressing ESC.
 * Use this when the user must interact with the modal content (e.g. re-authentication on timeout).
 */
@customElement('umb-persistent-modal-dialog')
export class UmbPersistentModalDialogElement extends UUIModalDialogElement {
	#abortController?: AbortController;

	protected override _openModal(): void {
		this.#abortController?.abort();
		this.#abortController = new AbortController();
		const signal = this.#abortController.signal;

		this._dialogElement?.showModal();
		this._dialogElement?.addEventListener('keydown', (e) => e.key === 'Escape' && e.preventDefault(), { signal });
		this._dialogElement?.addEventListener('cancel', (e) => e.preventDefault(), { signal });

		// Defer isOpen to avoid scheduling a Lit update during firstUpdated
		queueMicrotask(() => {
			this.isOpen = true;
		});
	}

	override forceClose(): void {
		this.#abortController?.abort();
		super.forceClose();
	}
}

export default UmbPersistentModalDialogElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-persistent-modal-dialog': UmbPersistentModalDialogElement;
	}
}
