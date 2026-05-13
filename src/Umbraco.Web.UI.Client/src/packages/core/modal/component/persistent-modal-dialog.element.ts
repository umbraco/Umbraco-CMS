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

		this._popoverElement?.showPopover();
		if (!this._popoverElement?.hasAttribute('tabindex')) {
			this._popoverElement?.setAttribute('tabindex', '-1');
		}
		this._popoverElement?.addEventListener('cancel', (e) => e.preventDefault(), { signal });
		document.addEventListener('keydown', this._onKeyDown, { signal });
		document.addEventListener('focus', this._onFocus, true);

		// Defer isOpen to avoid scheduling a Lit update during firstUpdated
		queueMicrotask(() => {
			this.isOpen = true;
		});
	}

	override forceClose(): void {
		this.#abortController?.abort();
		document.removeEventListener('keydown', this._onKeyDown);
		document.removeEventListener('focus', this._onFocus, true);
		super.forceClose();
	}

	private readonly _onKeyDown = (e: KeyboardEvent) => {
		if (e.key === 'Escape') {
			e.preventDefault();
		}
	};

	private readonly _onFocus = (e: FocusEvent) => {
		if (this.index !== 0) return;
		if (!this._popoverElement?.contains(e.target as Node)) {
			this._popoverElement?.focus();
		}
	};
}

export default UmbPersistentModalDialogElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-persistent-modal-dialog': UmbPersistentModalDialogElement;
	}
}
