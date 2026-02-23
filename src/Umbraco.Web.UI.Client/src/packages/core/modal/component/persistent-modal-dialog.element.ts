import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UUIModalDialogElement } from '@umbraco-cms/backoffice/external/uui';

/**
 * A modal dialog element that cannot be dismissed by pressing ESC.
 * Use this when the user must interact with the modal content (e.g. re-authentication on timeout).
 */
@customElement('umb-persistent-modal-dialog')
export class UmbPersistentModalDialogElement extends UUIModalDialogElement {
	protected override _openModal(): void {
		this._dialogElement?.showModal();
		this._dialogElement?.addEventListener('keydown', (e) => {
			if (e.key === 'Escape') {
				e.preventDefault();
			}
		});
		// Defer isOpen to avoid scheduling a Lit update during firstUpdated
		queueMicrotask(() => {
			this.isOpen = true;
		});
	}
}

export default UmbPersistentModalDialogElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-persistent-modal-dialog': UmbPersistentModalDialogElement;
	}
}
