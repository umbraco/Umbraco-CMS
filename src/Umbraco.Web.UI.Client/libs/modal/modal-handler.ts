import type { UUIDialogElement } from '@umbraco-ui/uui';
import type { UUIModalDialogElement } from '@umbraco-ui/uui-modal-dialog';
import type { UUIModalSidebarElement, UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { v4 as uuidv4 } from 'uuid';
import { BehaviorSubject } from 'rxjs';
import { UmbModalConfig, UmbModalType } from './modal.context';
import { UmbModalToken } from './token/modal-token';
import { createExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { UmbObserverController } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ManifestModal } from '@umbraco-cms/extensions-registry';

//TODO consider splitting this into two separate handlers
export class UmbModalHandler {
	private _closeResolver: any;
	private _closePromise: any;
	#host: UmbControllerHostInterface;

	public containerElement: UUIModalDialogElement | UUIModalSidebarElement;

	#element = new BehaviorSubject<unknown | undefined>(undefined);
	public readonly element = this.#element.asObservable();

	public key: string;
	public type: UmbModalType = 'dialog';
	public size: UUIModalSidebarSize = 'small';

	constructor(
		host: UmbControllerHostInterface,
		modalAlias: string | UmbModalToken,
		data?: unknown,
		config?: UmbModalConfig
	) {
		this.#host = host;
		this.key = uuidv4();

		if (modalAlias instanceof UmbModalToken) {
			this.type = modalAlias.getDefaultConfig().type || this.type;
			this.size = modalAlias.getDefaultConfig().size || this.size;
		}

		this.type = config?.type || this.type;
		this.size = config?.size || this.size;

		// TODO: Consider if its right to use Promises, or use another event based system? Would we need to be able to cancel an event, to then prevent the closing..?
		this._closePromise = new Promise((resolve) => {
			this._closeResolver = resolve;
		});

		this.containerElement = this.#createContainerElement();
		this.#observeModal(modalAlias.toString(), data);
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

	async #createLayoutElement(manifest: ManifestModal, data?: unknown) {
		// TODO: add fallback element if no layout is found
		const layoutElement = (await createExtensionElement(manifest)) as any;

		if (layoutElement) {
			layoutElement.data = data;
			layoutElement.modalHandler = this;
		}

		return layoutElement;
	}

	public close(...args: any) {
		this._closeResolver(...args);
		this.containerElement.close();
	}

	public onClose(): Promise<any> {
		return this._closePromise;
	}

	/* TODO: modals being part of the extension registry know means that a modal element can change over time.
	 It makes this code a bit more complex. The main idea is to have the element as part of the modalHandler so it is possible to dispatch events from within the modal element to the one that opened it.
	 Now when the element is an observable it makes it more complex because this host needs to subscribe to updates to the element, instead of just having a reference to it.
	 If we find a better generic solution to communicate between the modal and the host, then we can remove the element as part of the modalHandler. */
	#observeModal(modalAlias: string, data?: unknown) {
		new UmbObserverController(
			this.#host,
			umbExtensionsRegistry.getByTypeAndAlias('modal', modalAlias),
			async (manifest) => {
				if (manifest) {
					const element = await this.#createLayoutElement(manifest, data);
					this.#element.next(element);
					this.containerElement.appendChild(element);
				} else {
					this.#element.next(undefined);
				}
			}
		);
	}
}
