import type { UmbModalContext } from '../context/modal.context.js';
import { UMB_MODAL_CONTEXT } from '../context/modal.context-token.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbBasicState, type UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import {
	UUIModalCloseEvent,
	type UUIModalElement,
	type UUIDialogElement,
	type UUIModalDialogElement,
	type UUIModalSidebarElement,
	type UUIModalSidebarSize,
} from '@umbraco-cms/backoffice/external/uui';
import { UMB_ROUTE_CONTEXT, type UmbRouterSlotElement } from '@umbraco-cms/backoffice/router';
import { createExtensionElement, loadManifestElement } from '@umbraco-cms/backoffice/extension-api';
import type { UmbContextRequestEvent } from '@umbraco-cms/backoffice/context-api';
import {
	UMB_CONTEXT_REQUEST_EVENT_TYPE,
	UmbContextBoundary,
	UmbContextProvider,
} from '@umbraco-cms/backoffice/context-api';

@customElement('umb-modal')
export class UmbModalElement extends UmbLitElement {
	#modalContext?: UmbModalContext;

	public element?: UUIModalDialogElement | UUIModalSidebarElement | UUIModalElement;

	#innerElement = new UmbBasicState<HTMLElement | undefined>(undefined);

	#modalExtensionObserver?: UmbObserverController<ManifestModal | undefined>;
	#modalRouterElement?: HTMLDivElement | UmbRouterSlotElement;

	#onClose = (e: Event) => {
		if (this.#modalContext?.isResolved() === false) {
			// If not resolved, and has a router, then we do not want to close, but instead let the router try to change the path for that to eventually trigger another round of close.
			if (this.#modalContext?.router) {
				e.stopImmediatePropagation();
				e.preventDefault();
				this.#modalContext._internal_removeCurrentModal();
				return;
			}
		}
		this.element?.removeEventListener(UUIModalCloseEvent, this.#onClose);
		this.#modalContext?.reject({ type: 'close' });
	};

	async init(modalContext: UmbModalContext | undefined) {
		if (this.#modalContext === modalContext) return;
		this.#modalContext = modalContext;

		if (!this.#modalContext) {
			this.destroy();
			return;
		}

		this.#modalContext.addEventListener('umb:destroy', this.#onContextDestroy);
		this.element = await this.#createContainerElement();

		// Makes sure that the modal triggers the reject of the context promise when it is closed by pressing escape.
		this.element.addEventListener(UUIModalCloseEvent, this.#onClose);

		// The following code is the context api proxy.
		// It re-dispatches the context api request event to the origin target of this modal, in other words the element that initiated the modal. [NL]
		this.element.addEventListener(UMB_CONTEXT_REQUEST_EVENT_TYPE, ((event: UmbContextRequestEvent) => {
			if (!this.#modalContext) return;
			// Note for this hack (The if-sentence):  [NL]
			// We do not currently have a good enough control to ensure that the proxy is last, meaning if another context is provided at this element, it might respond after the proxy event has been dispatched.
			// To avoid such this hack just prevents proxying the event if its a request for the Modal Context. [NL]
			if (event.contextAlias !== UMB_MODAL_CONTEXT.contextAlias) {
				event.stopImmediatePropagation();
				this.#modalContext.getHostElement().dispatchEvent(event.clone());
			}
		}) as EventListener);

		this.#modalContext.onSubmit().then(
			() => {
				this.element?.close();
			},
			() => {
				this.element?.close();
			},
		);

		/**
		 *
		 * Maybe we could just get a Modal Router Slot. But it needs to have the ability to actually inject via slot. so the modal inner element can be within.
		 *
		 */
		if (this.#modalContext.router) {
			this.#modalRouterElement = document.createElement('umb-router-slot');
			this.#modalRouterElement.routes = [
				{
					path: '',
					component: document.createElement('slot'),
				},
			];
			this.#modalRouterElement.parent = this.#modalContext.router;
		} else {
			this.#modalRouterElement = document.createElement('div');
			// Notice inline styling here is used cause the element is not appended into this elements shadowDom but outside and there by gets into the element via a slot.
			this.#modalRouterElement.style.position = 'relative';
			this.#modalRouterElement.style.height = '100%';
			new UmbContextBoundary(this.#modalRouterElement, UMB_ROUTE_CONTEXT).hostConnected();
		}

		this.element.appendChild(this.#modalRouterElement);

		this.#observeModal(this.#modalContext.alias.toString());

		const provider = new UmbContextProvider(this.element, UMB_MODAL_CONTEXT, this.#modalContext);
		provider.hostConnected();
	}

	async #createContainerElement() {
		if (this.#modalContext!.type == 'custom' && this.#modalContext?.element) {
			const customWrapperElementCtor = await loadManifestElement(this.#modalContext.element);
			return new customWrapperElementCtor!();
		}

		return this.#modalContext!.type === 'sidebar' ? this.#createSidebarElement() : this.#createDialogElement();
	}

	#createSidebarElement() {
		const sidebarElement = document.createElement('uui-modal-sidebar');
		this.observe(
			this.#modalContext!.size,
			(size) => {
				sidebarElement.size = size as UUIModalSidebarSize;
			},
			'observeSize',
		);
		return sidebarElement;
	}

	#createDialogElement() {
		const modalDialogElement = document.createElement('uui-modal-dialog');
		const dialogElement: UUIDialogElement = document.createElement('uui-dialog');
		modalDialogElement.appendChild(dialogElement);
		return modalDialogElement;
	}

	#observeModal(alias: string) {
		this.#modalExtensionObserver?.destroy();

		this.observe(umbExtensionsRegistry.byTypeAndAlias('modal', alias), async (manifest) => {
			this.#removeInnerElement();

			if (manifest) {
				const innerElement = await this.#createInnerElement(manifest);
				if (innerElement) {
					this.#appendInnerElement(innerElement);
				}
			}
		});
	}

	async #createInnerElement(manifest: ManifestModal) {
		// TODO: add inner fallback element if no extension element is found
		const innerElement = await createExtensionElement(manifest);

		if (!this.#modalContext) {
			// If context does not exist any more, it means we have been destroyed. So we need to back out:
			return undefined;
		}
		if (innerElement) {
			innerElement.manifest = manifest;
			innerElement.data = this.#modalContext.data;
			innerElement.modalContext = this.#modalContext;
		}

		return innerElement;
	}

	#appendInnerElement(element: HTMLElement) {
		this.#modalRouterElement!.appendChild(element);
		this.#innerElement.setValue(element);
	}

	#removeInnerElement() {
		const innerElement = this.#innerElement.getValue();
		if (innerElement) {
			this.#modalRouterElement!.removeChild(innerElement);
			this.#innerElement.setValue(undefined);
		}
	}

	override render() {
		return html`${this.element}`;
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.destroy();
	}

	#onContextDestroy = () => {
		this.destroy();
	};

	override destroy() {
		this.#innerElement.destroy();
		this.#modalExtensionObserver?.destroy();
		this.#modalExtensionObserver = undefined;
		if (this.#modalContext) {
			this.#modalContext.removeEventListener('umb:destroy', this.#onContextDestroy);
			this.#modalContext.destroy();
			this.#modalContext = undefined;
		}
		super.destroy();
	}

	static override styles: CSSResultGroup = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal': UmbModalElement;
	}
}
