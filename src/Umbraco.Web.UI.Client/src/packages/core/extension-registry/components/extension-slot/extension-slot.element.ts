import { umbExtensionsRegistry } from '../../registry.js';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import { css, repeat, customElement, property, state, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import {
	type UmbExtensionElementInitializer,
	UmbExtensionsElementInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-extension-slot
 * @description A element which renderers the extensions of a given type or types.
 * @slot default - slot for inserting additional things into this slot.
 * @class UmbExtensionSlot
 * @augments {UmbLitElement}
 */

// TODO: Refactor extension-slot and extension-with-api slot.
// TODO: Fire change event.
// TODO: Make property that reveals the amount of displayed/permitted extensions.
@customElement('umb-extension-slot')
export class UmbExtensionSlotElement extends UmbLitElement {
	#attached = false;
	#extensionsController?: UmbExtensionsElementInitializer | UmbExtensionElementInitializer;
	#disconnectTimeoutId?: number;

	@state()
	private _permitted?: Array<UmbExtensionElementInitializer>;

	@property({ type: Boolean })
	single?: boolean;

	/**
	 * The type or types of extensions to render.
	 * @type {string | string[]}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-slot type="my-extension-type"></umb-extension-slot>
	 * or multiple:
	 * <umb-extension-slot .type=${['my-extension-type','another-extension-type']}></umb-extension-slot>
	 */
	@property({ type: String })
	public set type(value: string | string[] | undefined) {
		if (value === this.#type) return;
		this.#type = value;
		this.#observeExtensions();
	}
	public get type(): string | string[] | undefined {
		return this.#type;
	}
	#type?: string | string[] | undefined;

	/**
	 * Filter method for extension manifests.
	 * This is an initial filter taking effect before conditions or overwrites, the extensions will still be filtered by the conditions defined in the manifest.
	 * @type {(manifest: any) => boolean}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-slot type="my-extension-type" .filter=${(ext) => ext.meta.anyPropToFilter === 'foo'}></umb-extension-slot>
	 */
	@property({ type: Object, attribute: false })
	public set filter(value: (manifest: any) => boolean) {
		if (value === this.#filter) return;
		this.#filter = value;
		this.#observeExtensions();
	}
	public get filter(): (manifest: any) => boolean {
		return this.#filter;
	}
	#filter: (manifest: any) => boolean = () => true;

	/**
	 * Properties to pass to the extensions elements.
	 * Notice: The individual manifest of the extension is parsed to each extension element no matter if this is set or not.
	 * @type {Record<string, any>}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-slot type="my-extension-type" .props=${{foo: 'bar'}}></umb-extension-slot>
	 */
	@property({ type: Object, attribute: false })
	set props(newVal: Record<string, unknown> | undefined) {
		this.#props = newVal;
		if (this.#extensionsController) {
			this.#extensionsController.properties = newVal;
		}
	}
	get props(): Record<string, unknown> | undefined {
		return this.#props;
	}
	#props?: Record<string, unknown> = {};

	@property({ type: Object, attribute: false })
	set events(newVal: Record<string, (event: Event) => void> | undefined) {
		this.#events = newVal;
		if (this.#extensionsController) {
			this.#addEventListenersToExtensionElement();
		}
	}
	get events(): Record<string, (event: Event) => void> | undefined {
		return this.#events;
	}
	#events?: Record<string, (event: Event) => void> = {};

	@property({ type: String, attribute: 'default-element' })
	public defaultElement?: string;

	@property({ attribute: false })
	public renderMethod?: (
		extension: UmbExtensionElementInitializer,
		index: number,
	) => TemplateResult | TemplateResult<1> | HTMLElement | null | undefined | typeof nothing;

	@property({ attribute: false })
	public fallbackRenderMethod?: () =>
		| TemplateResult
		| TemplateResult<1>
		| HTMLElement
		| null
		| undefined
		| typeof nothing;

	override connectedCallback(): void {
		super.connectedCallback();
		// Cancel any pending destruction if we're being reconnected (e.g., during a DOM move/sort)
		if (this.#disconnectTimeoutId !== undefined) {
			cancelAnimationFrame(this.#disconnectTimeoutId);
			this.#disconnectTimeoutId = undefined;
			// Only skip re-initialization if the controller still exists
			if (this.#extensionsController) {
				this.#attached = true;
				return;
			}
		}
		this.#attached = true;
		this.#observeExtensions();
	}
	override disconnectedCallback(): void {
		this.#attached = false;
		// Clear any existing pending frame request (defensive cleanup)
		if (this.#disconnectTimeoutId !== undefined) {
			cancelAnimationFrame(this.#disconnectTimeoutId);
		}
		// Defer destruction to allow for reconnection during DOM moves/sorting
		// If reconnected before the next frame, the destruction is cancelled
		this.#disconnectTimeoutId = requestAnimationFrame(() => {
			this.#disconnectTimeoutId = undefined;
			// Only destroy if still detached
			if (!this.#attached) {
				this.#removeEventListenersFromExtensionElement();
				this.#extensionsController?.destroy();
				this.#extensionsController = undefined;
			}
		});
		super.disconnectedCallback();
	}

	#observeExtensions(): void {
		if (!this.#attached) return;
		this.#extensionsController?.destroy();
		if (this.#type) {
			this.#extensionsController = new UmbExtensionsElementInitializer(
				this,
				umbExtensionsRegistry,
				this.#type,
				this.filter,
				(extensionControllers) => {
					this._permitted = extensionControllers;
					this.#addEventListenersToExtensionElement();
				},
				undefined, // We can leave the alias undefined as we destroy this our selfs.
				this.defaultElement,
				{
					single: this.single,
				},
			);
			this.#extensionsController.properties = this.#props;
		}
	}

	override render() {
		// First renders something once _permitted is set, this is to avoid flickering. [NL]
		return this._permitted
			? this._permitted.length > 0
				? repeat(this._permitted, (ext) => ext.alias, this.#renderExtension)
				: this.#renderNothing()
			: nothing;
	}

	#renderNothing() {
		return this.fallbackRenderMethod ? this.fallbackRenderMethod() : html`<slot></slot>`;
	}

	#renderExtension = (ext: UmbExtensionElementInitializer, i: number) => {
		return this.renderMethod ? this.renderMethod(ext, i) : ext.component;
	};

	#addEventListenersToExtensionElement() {
		this._permitted?.forEach((initializer) => {
			const component = initializer.component as HTMLElement;
			if (!component) return;

			const events = this.#events;
			if (!events) return;

			this.#removeEventListenersFromExtensionElement();

			Object.entries(events).forEach(([eventName, handler]) => {
				component.addEventListener(eventName, handler);
			});
		});
	}

	#removeEventListenersFromExtensionElement() {
		this._permitted?.forEach((initializer) => {
			const component = initializer.component as HTMLElement;
			if (!component) return;

			const events = this.#events;
			if (!events) return;

			Object.entries(events).forEach(([eventName, handler]) => {
				component.removeEventListener(eventName, handler);
			});
		});
	}

	static override styles = css`
		:host {
			display: contents;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-slot': UmbExtensionSlotElement;
	}
}
