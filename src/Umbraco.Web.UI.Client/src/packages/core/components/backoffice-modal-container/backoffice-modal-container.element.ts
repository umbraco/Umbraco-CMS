import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, repeat, customElement, state, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbModalManagerContext, UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalElement } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-backoffice-modal-container')
export class UmbBackofficeModalContainerElement extends UmbLitElement {
	@state()
	private _modalElementMap: Map<string, UmbModalElement> = new Map();

	@state()
	_modals: Array<UmbModalContext> = [];

	@property({ type: Boolean, reflect: true, attribute: 'fill-background' })
	fillBackground = false;

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

	/**
	 * We cannot render the umb-modal element directly in the uui-modal-container because it wont get recognized by UUI.
	 *  We therefore have a helper class which creates the uui-modal element and returns it.
	 * @param modals
	 */
	#createModalElements(modals: Array<UmbModalContext>) {
		this.fillBackground = false;
		const oldValue = this._modals;
		this._modals = modals;

		const oldModals = oldValue.filter((oldModal) => !modals.some((modal) => modal.key === oldModal.key));

		oldModals.forEach((modal) => {
			// TODO: I would not think this works as expected, the callback method has to be the exact same instance as the one added: [NL]
			const modalElement = this._modalElementMap.get(modal.key);
			modalElement?.removeEventListener('close-end', this.#onCloseEnd.bind(this, modal.key));
			modalElement?.destroy();
			this._modalElementMap.delete(modal.key);
			modal.destroy();
		});

		if (this._modals.length === 0) {
			//this._modalElementMap.clear(); // should not make a difference now that we clean it above. [NL]
			return;
		}

		this._modals.forEach(async (modalContext) => {
			if (this._modalElementMap.has(modalContext.key)) return;

			const modalElement = new UmbModalElement();
			await modalElement.init(modalContext);

			modalElement.element?.addEventListener('close-end', this.#onCloseEnd.bind(this, modalContext.key));
			modalContext.addEventListener('umb:destroy', this.#onCloseEnd.bind(this, modalContext.key));

			this._modalElementMap.set(modalContext.key, modalElement);

			// If any of the modals are fillBackground, set the fillBackground property to true
			if (modalContext.backdropBackground) {
				this.fillBackground = true;
				this.shadowRoot
					?.getElementById('container')
					?.style.setProperty('--backdrop-background', modalContext.backdropBackground);
			}

			this.requestUpdate('_modalElementMap');
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

	override render() {
		return html`
			<uui-modal-container id="container">
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

	static override styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			:host {
				position: absolute;
				z-index: 1000;
			}

			:host([fill-background]) {
				--uui-modal-dialog-border-radius: 0;
				--uui-shadow-depth-5: 0;
			}

			:host([fill-background]) #container::after {
				background: var(--backdrop-background);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-modal-container': UmbBackofficeModalContainerElement;
	}
}
