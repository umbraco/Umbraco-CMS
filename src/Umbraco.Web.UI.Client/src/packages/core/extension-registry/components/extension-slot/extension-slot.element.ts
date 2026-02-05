import { umbExtensionsRegistry } from '../../registry.js';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import { css, repeat, customElement, property, state, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import {
	type UmbExtensionElementInitializer,
	UmbExtensionsElementInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * A custom element that dynamically renders extensions registered in the extension registry.
 *
 * This element observes the extension registry and renders all permitted extensions matching the specified type(s).
 * Extensions are automatically filtered by their conditions and can be further filtered using the `filter` property.
 * The element handles extension lifecycle, property passing, and event binding.
 * @element umb-extension-slot
 * @slot default - Fallback content shown when no extensions are permitted (unless `fallbackRenderMethod` is provided).
 * @example Basic usage - Render all extensions of a type
 * ```html
 * <umb-extension-slot type="workspaceFooterApp"></umb-extension-slot>
 * ```
 * @example Multiple types - Render extensions from multiple types
 * ```html
 * <umb-extension-slot .type=${['headerApp', 'headerAppButton']}></umb-extension-slot>
 * ```
 * @example Single extension - Only render the first matching extension
 * ```html
 * <umb-extension-slot type="menu" single></umb-extension-slot>
 * ```
 * @example With filter - Filter extensions by manifest properties
 * ```html
 * <umb-extension-slot
 *     type="menu"
 *     .filter=${(manifest) => manifest.alias === 'My.Menu.Alias'}>
 * </umb-extension-slot>
 * ```
 * @example With props - Pass data to extension elements
 * ```html
 * <umb-extension-slot
 *     type="searchResultItem"
 *     .props=${{ item: searchResult }}>
 * </umb-extension-slot>
 * ```
 * @example With default element - Specify fallback element for extensions without one
 * ```html
 * <umb-extension-slot
 *     type="menu"
 *     default-element="umb-menu">
 * </umb-extension-slot>
 * ```
 * @example Combined usage - Filter, props, default element, and single
 * ```html
 * <umb-extension-slot
 *     type="searchResultItem"
 *     single
 *     .filter=${(manifest) => manifest.forEntityTypes.includes(item.entityType)}
 *     .props=${{ item }}
 *     default-element="umb-search-result-item">
 * </umb-extension-slot>
 * ```
 * @example Custom render method - Control how extensions are rendered
 * ```html
 * <umb-extension-slot
 *     type="blockEditorCustomView"
 *     single
 *     .filter=${this.#extensionSlotFilterMethod}
 *     .renderMethod=${(ext, index) => html`<div class="wrapper">${ext.component}</div>`}
 *     .props=${this._blockViewProps}>
 * </umb-extension-slot>
 * ```
 * @example Fallback content - Shows the slotted content when no extensions match
 * ```html
 * <umb-extension-slot type="myExtensionType">
 *     <p>No extensions available</p>
 * </umb-extension-slot>
 * ```
 * @example Fallback render method - Shows the result of the fallbackRenderMethod when no extensions match
 * ```html
 * <umb-extension-slot type="myExtensionType">
 *     .fallbackRenderMethod=${() => html`<p>No extensions available</p>`}
 * </umb-extension-slot>
 * ```
 */

// TODO: Refactor extension-slot and extension-with-api slot.
// TODO: Fire change event.
// TODO: Make property that reveals the amount of displayed/permitted extensions.
@customElement('umb-extension-slot')
export class UmbExtensionSlotElement extends UmbLitElement {
	#attached = false;
	#extensionsController?: UmbExtensionsElementInitializer | UmbExtensionElementInitializer;

	@state()
	private _permitted?: Array<UmbExtensionElementInitializer>;

	/**
	 * When true, only renders the highest weighted permitted extension.
	 * Useful for extension types where only one instance should be displayed (e.g., menus).
	 * @example
	 * ```html
	 * <umb-extension-slot type="menu" single></umb-extension-slot>
	 * ```
	 */
	@property({ type: Boolean })
	single?: boolean;

	/**
	 * The type or types of extensions to render. Required for the slot to display anything.
	 * Can be a single type string or an array of type strings to render extensions from multiple types.
	 * @example Single type
	 * ```html
	 * <umb-extension-slot type="workspaceFooterApp"></umb-extension-slot>
	 * ```
	 * @example Multiple types
	 * ```html
	 * <umb-extension-slot .type=${['my-extension-type','another-extension-type']}></umb-extension-slot>
	 * ```
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
	 * Filter function for extension manifests.
	 * This is an initial filter taking effect before conditions or overwrites are applied.
	 * Extensions will still be filtered by their manifest-defined conditions after this filter.
	 * The filter function receives the extension manifest and should return true to include it.
	 * @example Filter by manifest alias
	 * ```html
	 * <umb-extension-slot
	 *     type="menu"
	 *     .filter=${(manifest) => manifest.alias === 'My.Menu.Alias'}>
	 * </umb-extension-slot>
	 * ```
	 * @example Filter by meta properties
	 * ```html
	 * <umb-extension-slot
	 *     type="searchResultItem"
	 *     .filter=${(manifest) => manifest.forEntityTypes.includes(entityType)}>
	 * </umb-extension-slot>
	 * ```
	 */
	@property({ type: Object, attribute: false })
	public set filter(value: (manifest: unknown) => boolean) {
		if (value === this.#filter) return;
		this.#filter = value;
		this.#observeExtensions();
	}
	public get filter(): (manifest: unknown) => boolean {
		return this.#filter;
	}
	#filter: (manifest: unknown) => boolean = () => true;

	/**
	 * Properties to pass to all rendered extension elements.
	 * These properties are spread onto each extension element instance.
	 * Note: The extension's manifest is always passed automatically regardless of this setting.
	 * @example Pass data to extension elements
	 * ```html
	 * <umb-extension-slot
	 *     type="searchResultItem"
	 *     .props=${{ item: searchResult, showDetails: true }}>
	 * </umb-extension-slot>
	 * ```
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

	/**
	 * Event listeners to attach to all rendered extension elements.
	 * The key is the event name, and the value is the event handler function.
	 * Listeners are automatically added when extensions are rendered and removed on disconnect.
	 * @example Listen for custom events from extensions
	 * ```html
	 * <umb-extension-slot
	 *     type="myExtensionType"
	 *     .events=${{
	 *         'item-selected': (e) => this.#handleItemSelected(e),
	 *         'action-clicked': (e) => this.#handleAction(e)
	 *     }}>
	 * </umb-extension-slot>
	 * ```
	 */
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

	/**
	 * Fallback element tag name to use when an extension manifest doesn't specify its own element.
	 * This allows extensions to rely on a default UI implementation while still being registered.
	 * @example Provide a default menu element
	 * ```html
	 * <umb-extension-slot
	 *     type="menu"
	 *     default-element="umb-menu">
	 * </umb-extension-slot>
	 * ```
	 */
	@property({ type: String, attribute: 'default-element' })
	public defaultElement?: string;

	/**
	 * Custom render function for controlling how each extension is rendered.
	 * When provided, this function is called for each permitted extension instead of the default rendering.
	 * The function receives the extension initializer (with `component` and `manifest` properties) and the index,
	 * and should return a TemplateResult, HTMLElement, null, or nothing.
	 * @example Wrap extensions in custom markup
	 * ```html
	 * <umb-extension-slot
	 *     type="blockEditorCustomView"
	 *     .renderMethod=${(ext, index) => html`
	 *         <div class="block-wrapper" data-index=${index}>
	 *             ${ext.component}
	 *         </div>
	 *     `}>
	 * </umb-extension-slot>
	 * ```
	 */
	@property({ attribute: false })
	public renderMethod?: (
		extension: UmbExtensionElementInitializer,
		index: number,
	) => TemplateResult | TemplateResult<1> | HTMLElement | null | undefined | typeof nothing;

	/**
	 * Render function called when no extensions are permitted.
	 * When provided, this function is called instead of rendering the default slot content.
	 * Useful for providing custom empty states or fallback UI.
	 * The function should return a TemplateResult, HTMLElement, null, or nothing.
	 * @example Custom empty state
	 * ```html
	 * <umb-extension-slot
	 *     type="blockEditorCustomView"
	 *     .fallbackRenderMethod=${() => html`<div class="default-view">Default block view</div>`}>
	 * </umb-extension-slot>
	 * ```
	 */
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
		this.#attached = true;
		this.#observeExtensions();
	}
	override disconnectedCallback(): void {
		// _permitted is reset as the extensionsController fires a callback on destroy.
		this.#removeEventListenersFromExtensionElement();
		this.#attached = false;
		this.#extensionsController?.destroy();
		this.#extensionsController = undefined;
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
