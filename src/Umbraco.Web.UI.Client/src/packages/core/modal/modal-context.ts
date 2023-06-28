import { UmbModalConfig, UmbModalType } from './modal-manager.context.js';
import { UmbModalToken } from './token/modal-token.js';
import type { IRouterSlot } from '@umbraco-cms/backoffice/external/router-slot';
import type {
	UUIDialogElement,
	UUIModalDialogElement,
	UUIModalSidebarElement,
	UUIModalSidebarSize,
} from '@umbraco-cms/backoffice/external/uui';
import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';
import { ManifestModal, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbRouterSlotElement } from '@umbraco-cms/backoffice/router';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHostElement, UmbController } from '@umbraco-cms/backoffice/controller-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextProvider, UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * Type which omits the real submit method, and replaces it with a submit method which accepts an optional argument depending on the generic type.
 */
export type UmbModalContext<ModalData extends object = object, ModalResult = any> = Omit<
	UmbModalContextClass<ModalData, ModalResult>,
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

// TODO: consider splitting this into two separate handlers
export class UmbModalContextClass<ModalData extends object = object, ModalResult = unknown> implements UmbController {
	#host: UmbControllerHostElement;

	#submitPromise: Promise<ModalResult>;
	#submitResolver?: (value: ModalResult) => void;
	#submitRejecter?: () => void;

	private _modalExtensionObserver?: UmbObserverController<ManifestModal | undefined>;
	public readonly modalElement: UUIModalDialogElement | UUIModalSidebarElement;
	#modalRouterElement: UmbRouterSlotElement = document.createElement('umb-router-slot');
	#modalContextProvider;

	#innerElement = new BehaviorSubject<HTMLElement | undefined>(undefined);
	public readonly innerElement = this.#innerElement.asObservable();

	public readonly key: string;
	public readonly data: ModalData;
	public readonly type: UmbModalType = 'dialog';
	public readonly size: UUIModalSidebarSize = 'small';

	public get unique() {
		return 'umbModalContext:' + this.key;
	}

	constructor(
		host: UmbControllerHostElement,
		router: IRouterSlot | null,
		modalAlias: string | UmbModalToken<ModalData, ModalResult>,
		data?: ModalData,
		config?: UmbModalConfig
	) {
		this.#host = host;
		this.key = config?.key || UmbId.new();

		if (modalAlias instanceof UmbModalToken) {
			this.type = modalAlias.getDefaultConfig()?.type || this.type;
			this.size = modalAlias.getDefaultConfig()?.size || this.size;
		}

		this.type = config?.type || this.type;
		this.size = config?.size || this.size;

		const defaultData = modalAlias instanceof UmbModalToken ? modalAlias.getDefaultData() : undefined;
		this.data = Object.freeze({ ...defaultData, ...data } as ModalData);

		// TODO: Consider if its right to use Promises, or use another event based system? Would we need to be able to cancel an event, to then prevent the closing..?
		this.#submitPromise = new Promise((resolve, reject) => {
			this.#submitResolver = resolve;
			this.#submitRejecter = reject;
		});

		this.modalElement = this.#createContainerElement();
		this.modalElement.addEventListener('close', () => {
			this.#submitRejecter?.();
		});

		/**
		 *
		 * Maybe we could just get a Modal Router Slot. But it needs to have the ability to actually inject via slot. so the modal inner element can be within.
		 *
		 */
		if (router) {
			this.#modalRouterElement.routes = [
				{
					path: '',
					component: document.createElement('slot'),
				},
			];
			this.#modalRouterElement.parent = router;
		}
		this.modalElement.appendChild(this.#modalRouterElement);
		this.#observeModal(modalAlias.toString());

		// Not using a controller, cause we want to use the modal as the provider, this is a UUI element. So its a bit of costume implementation:
		this.#modalContextProvider = new UmbContextProvider(
			this.modalElement,
			UMB_MODAL_CONTEXT_TOKEN,

			// Note, We are doing the Typing dance here because of the way we are correcting the submit method attribute type.
			this as unknown as UmbModalContext<ModalData, ModalResult>
		);

		this.#host.addController(this);
	}

	hostConnected(): void {
		this.#modalContextProvider.hostConnected();
	}
	hostDisconnected(): void {
		this.#modalContextProvider.hostDisconnected();
	}

	#createContainerElement() {
		return this.type === 'sidebar' ? this.#createSidebarElement() : this.#createDialogElement();
	}

	#createSidebarElement() {
		const sidebarElement = document.createElement('uui-modal-sidebar');
		sidebarElement.size = this.size;
		return sidebarElement;
	}

	#createDialogElement() {
		const modalDialogElement = document.createElement('uui-modal-dialog');
		const dialogElement: UUIDialogElement = document.createElement('uui-dialog');
		modalDialogElement.appendChild(dialogElement);
		return modalDialogElement;
	}

	/* TODO: modals being part of the extension registry now means that a modal element can change over time.
	 It makes this code a bit more complex. The main idea is to have the element as part of the modalContext so it is possible to dispatch events from within the modal element to the one that opened it.
	 Now when the element is an observable it makes it more complex because this host needs to subscribe to updates to the element, instead of just having a reference to it.
	 If we find a better generic solution to communicate between the modal and the implementor, then we can remove the element as part of the modalContext. */
	#observeModal(modalAlias: string) {
		if (this.#host) {
			this._modalExtensionObserver?.destroy();
			this._modalExtensionObserver = new UmbObserverController(
				this.#host,
				umbExtensionsRegistry.getByTypeAndAlias('modal', modalAlias),
				async (manifest) => {
					this.#removeInnerElement();
					if (manifest) {
						const innerElement = await this.#createInnerElement(manifest);
						if (innerElement) {
							this.#appendInnerElement(innerElement);
						}
					}
				}
			);
		}
	}

	async #createInnerElement(manifest: ManifestModal) {
		// TODO: add inner fallback element if no extension element is found
		const innerElement = (await createExtensionElement(manifest)) as any;

		if (innerElement) {
			innerElement.data = this.data;
			innerElement.modalContext = this;
			innerElement.manifest = manifest;
		}

		return innerElement;
	}

	#appendInnerElement(element: HTMLElement) {
		this.#modalRouterElement.appendChild(element);
		this.#innerElement.next(element);
	}

	#removeInnerElement() {
		const innerElement = this.#innerElement.getValue();
		if (innerElement) {
			this.#modalRouterElement.removeChild(innerElement);
			this.#innerElement.next(undefined);
		}
	}

	// note, this methods is private  argument is not defined correctly here, but requires to be fix by appending the OptionalSubmitArgumentIfUndefined type when newing up this class.
	/**
	 * Submits this modal, returning with a result to the initiator of the modal.
	 * @public
	 * @memberof UmbModalContext
	 */
	private submit(result?: ModalResult) {
		this.#submitResolver?.(result as ModalResult);
		this.modalElement.close();
	}

	/**
	 * Closes this modal
	 * @public
	 * @memberof UmbModalContext
	 */
	public reject() {
		this.modalElement.close();
	}

	/**
	 * Gives a Promise which will be resolved when this modal is submitted.
	 * @public
	 * @memberof UmbModalContext
	 */
	public onSubmit(): Promise<ModalResult> {
		return this.#submitPromise;
	}

	destroy(): void {
		this.#innerElement.complete();
		this._modalExtensionObserver?.destroy();
		this._modalExtensionObserver = undefined;
	}
}

export const UMB_MODAL_CONTEXT_TOKEN = new UmbContextToken<UmbModalContext>('UmbModalContext');
