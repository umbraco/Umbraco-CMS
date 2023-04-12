import type {
	UUIDialogElement,
	UUIModalDialogElement,
	UUIModalSidebarElement,
	UUIModalSidebarSize,
} from '@umbraco-ui/uui';
import { v4 as uuidv4 } from 'uuid';
import { BehaviorSubject } from 'rxjs';
import { UmbModalConfig, UmbModalType } from './modal.context';
import { UmbModalToken } from './token/modal-token';
import { createExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import type { ManifestModal } from '@umbraco-cms/backoffice/extensions-registry';

/**
 * Type which omits the real submit method, and replaces it with a submit method which accepts an optional argument depending on the generic type.
 */
export type UmbModalHandler<ModalData extends object = object, ModalResult = any> = Omit<
	UmbModalHandlerClass<ModalData, ModalResult>,
	'submit'
> &
	OptionalSubmitArgumentIfUndefined<ModalResult>;

// If Type is undefined we don't accept an argument,
// If type is unknown, we accept an option argument.
// If type is anything else, we require an argument of that type.
type OptionalSubmitArgumentIfUndefined<T> = T extends undefined
	? {
			submit: () => void;
	  }
	: T extends unknown
	? {
			submit: (arg?: T) => void;
	  }
	: {
			submit: (arg: T) => void;
	  };

//TODO consider splitting this into two separate handlers
export class UmbModalHandlerClass<ModalData extends object = object, ModalResult = unknown> {
	private _submitPromise: Promise<ModalResult>;
	private _submitResolver?: (value: ModalResult) => void;
	private _submitRejecter?: () => void;
	#host: UmbControllerHostElement;

	public modalElement: UUIModalDialogElement | UUIModalSidebarElement;

	#innerElement = new BehaviorSubject<HTMLElement | undefined>(undefined);
	public readonly innerElement = this.#innerElement.asObservable();

	#modalElement?: UUIModalSidebarElement | UUIDialogElement;

	public key: string;
	public type: UmbModalType = 'dialog';
	public size: UUIModalSidebarSize = 'small';

	constructor(
		host: UmbControllerHostElement,
		modalAlias: string | UmbModalToken<ModalData, ModalResult>,
		data?: ModalData,
		config?: UmbModalConfig
	) {
		this.#host = host;
		this.key = config?.key || uuidv4();

		if (modalAlias instanceof UmbModalToken) {
			this.type = modalAlias.getDefaultConfig()?.type || this.type;
			this.size = modalAlias.getDefaultConfig()?.size || this.size;
		}

		this.type = config?.type || this.type;
		this.size = config?.size || this.size;

		// TODO: Consider if its right to use Promises, or use another event based system? Would we need to be able to cancel an event, to then prevent the closing..?
		this._submitPromise = new Promise((resolve, reject) => {
			this._submitResolver = resolve;
			this._submitRejecter = reject;
		});

		this.modalElement = this.#createContainerElement();
		this.#observeModal(modalAlias.toString(), data);
	}

	#createContainerElement() {
		return this.type === 'sidebar' ? this.#createSidebarElement() : this.#createDialogElement();
	}

	#createSidebarElement() {
		const sidebarElement = document.createElement('uui-modal-sidebar');
		this.#modalElement = sidebarElement;
		sidebarElement.size = this.size;
		return sidebarElement;
	}

	#createDialogElement() {
		const modalDialogElement = document.createElement('uui-modal-dialog');
		const dialogElement: UUIDialogElement = document.createElement('uui-dialog');
		this.#modalElement = dialogElement;
		modalDialogElement.appendChild(dialogElement);
		return modalDialogElement;
	}

	async #createInnerElement(manifest: ManifestModal, data?: ModalData) {
		// TODO: add inner fallback element if no extension element is found
		const innerElement = (await createExtensionElement(manifest)) as any;

		if (innerElement) {
			innerElement.data = data;
			//innerElement.observable = this.#dataObservable;
			innerElement.modalHandler = this;
		}

		return innerElement;
	}

	// note, this methods is private  argument is not defined correctly here, but requires to be fix by appending the OptionalSubmitArgumentIfUndefined type when newing up this class.
	private submit(result?: ModalResult) {
		this._submitResolver?.(result as ModalResult);
		this.modalElement.close();
	}

	public reject() {
		this._submitRejecter?.();
		this.modalElement.close();
	}

	public onSubmit(): Promise<ModalResult> {
		return this._submitPromise;
	}

	/* TODO: modals being part of the extension registry now means that a modal element can change over time.
	 It makes this code a bit more complex. The main idea is to have the element as part of the modalHandler so it is possible to dispatch events from within the modal element to the one that opened it.
	 Now when the element is an observable it makes it more complex because this host needs to subscribe to updates to the element, instead of just having a reference to it.
	 If we find a better generic solution to communicate between the modal and the implementor, then we can remove the element as part of the modalHandler. */
	#observeModal(modalAlias: string, data?: ModalData) {
		new UmbObserverController(
			this.#host,
			umbExtensionsRegistry.getByTypeAndAlias('modal', modalAlias),
			async (manifest) => {
				if (manifest) {
					const innerElement = await this.#createInnerElement(manifest, data);
					if (innerElement) {
						this.#appendInnerElement(innerElement);
						return;
					}
				}
				this.#removeInnerElement();
			}
		);
	}

	#appendInnerElement(element: HTMLElement) {
		this.#modalElement?.appendChild(element);
		this.#innerElement.next(element);
	}

	#removeInnerElement() {
		const innerElement = this.#innerElement.getValue();
		if (innerElement) {
			this.#modalElement?.removeChild(innerElement);
			this.#innerElement.next(undefined);
		}
	}
}
