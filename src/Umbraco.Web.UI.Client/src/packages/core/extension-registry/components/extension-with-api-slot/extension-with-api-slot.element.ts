import { umbExtensionsRegistry } from '../../registry.js';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import { css, repeat, customElement, property, state, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import {
	type UmbExtensionElementAndApiInitializer,
	UmbExtensionsElementAndApiInitializer,
	type UmbApiConstructorArgumentsMethodType,
	type ApiLoaderProperty,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * A custom element that dynamically renders extensions with both UI elements and API classes.
 *
 * Similar to `umb-extension-slot`, this element observes the extension registry and renders permitted extensions.
 * The key difference is that this slot also initializes an API class for each extension, allowing extensions
 * to have both a visual component and associated business logic. The API instance is created with configurable
 * constructor arguments and can receive properties.
 * @element umb-extension-with-api-slot
 * @slot default - Fallback content shown when no extensions are permitted (unless `fallbackRenderMethod` is provided).
 * @example Basic usage - Render extensions with APIs
 * ```html
 * <umb-extension-with-api-slot type="entityAction"></umb-extension-with-api-slot>
 * ```
 * @example Multiple types - Render extensions from multiple types
 * ```html
 * <umb-extension-with-api-slot .type=${['workspaceAction', 'workspaceActionMenuItem']}></umb-extension-with-api-slot>
 * ```
 * @example Single extension - Only render the first matching extension
 * ```html
 * <umb-extension-with-api-slot type="collectionTextFilter" single></umb-extension-with-api-slot>
 * ```
 * @example With filter - Filter extensions by manifest properties
 * ```html
 * <umb-extension-with-api-slot
 *     type="entityAction"
 *     .filter=${(manifest) => manifest.forEntityTypes.includes(entityType)}>
 * </umb-extension-with-api-slot>
 * ```
 * @example With element props - Pass data to extension elements
 * ```html
 * <umb-extension-with-api-slot
 *     type="entityAction"
 *     .elementProps=${{ entityType, unique }}>
 * </umb-extension-with-api-slot>
 * ```
 * @example With API args - Pass constructor arguments to extension APIs
 * ```html
 * <umb-extension-with-api-slot
 *     type="workspaceAction"
 *     .apiArgs=${[workspaceContext]}>
 * </umb-extension-with-api-slot>
 * ```
 * @example With API args method - Dynamic constructor arguments based on manifest
 * ```html
 * <umb-extension-with-api-slot
 *     type="workspaceAction"
 *     .apiArgs=${(manifest) => [workspaceContext, manifest.meta]}>
 * </umb-extension-with-api-slot>
 * ```
 * @example Combined usage - Filter, element props, and API args
 * ```html
 * <umb-extension-with-api-slot
 *     type="entityAction"
 *     .filter=${this._filter}
 *     .elementProps=${this._props}
 *     .apiArgs=${this._apiArgs}
 *     .renderMethod=${(ext, index) => ext.component}>
 * </umb-extension-with-api-slot>
 * ```
 * @example Custom render method - Control how extensions are rendered
 * ```html
 * <umb-extension-with-api-slot
 *     type="menuItem"
 *     .renderMethod=${(ext, index) => html`
 *         <div class="menu-item-wrapper">
 *             ${ext.component}
 *         </div>
 *     `}>
 * </umb-extension-with-api-slot>
 * ```
 * @example Fallback content - Shows the slotted content when no extensions match
 * ```html
 * <umb-extension-with-api-slot type="myExtensionType">
 *     <p>No actions available</p>
 * </umb-extension-with-api-slot>
 * ```
 * @example Fallback render method - Shows the result of the fallbackRenderMethod when no extensions match
 * ```html
 * <umb-extension-with-api-slot type="myExtensionType">
 *     .fallbackRenderMethod=${() => html`<p>No extensions available</p>`}
 * </umb-extension-with-api-slot>
 * ```
 */

// TODO: Refactor extension-slot and extension-with-api slot.
// TODO: Fire change event.
// TODO: Make property that reveals the amount of displayed/permitted extensions.
@customElement('umb-extension-with-api-slot')
export class UmbExtensionWithApiSlotElement extends UmbLitElement {
	#attached = false;
	#extensionsController?: UmbExtensionsElementAndApiInitializer;

	@state()
	private _permitted?: Array<UmbExtensionElementAndApiInitializer>;

	/**
	 * When true, only renders the highest weighted permitted extension.
	 * Useful for extension types where only one instance should be displayed.
	 * @example
	 * ```html
	 * <umb-extension-with-api-slot type="collectionTextFilter" single></umb-extension-with-api-slot>
	 * ```
	 */
	@property({ type: Boolean })
	single?: boolean;

	/**
	 * The type or types of extensions to render. Required for the slot to display anything.
	 * Can be a single type string or an array of type strings to render extensions from multiple types.
	 * @example Single type
	 * ```html
	 * <umb-extension-with-api-slot type="entityAction"></umb-extension-with-api-slot>
	 * ```
	 * @example Multiple types
	 * ```html
	 * <umb-extension-with-api-slot .type=${['workspaceAction', 'workspaceActionMenuItem']}></umb-extension-with-api-slot>
	 * ```
	 */
	@property({ type: String })
	public get type(): string | string[] | undefined {
		return this.#type;
	}
	public set type(value: string | string[] | undefined) {
		if (value === this.#type) return;
		this.#type = value;
		this.#observeExtensions();
	}
	#type?: string | string[] | undefined;

	/**
	 * Filter function for extension manifests.
	 * This is an initial filter taking effect before conditions or overwrites are applied.
	 * Extensions will still be filtered by their manifest-defined conditions after this filter.
	 * The filter function receives the extension manifest and should return true to include it.
	 * @example Filter by entity type
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="entityAction"
	 *     .filter=${(manifest) => manifest.forEntityTypes.includes(entityType)}>
	 * </umb-extension-with-api-slot>
	 * ```
	 * @example Filter by meta properties
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="workspaceAction"
	 *     .filter=${(manifest) => manifest.meta.look === 'primary'}>
	 * </umb-extension-with-api-slot>
	 * ```
	 */
	@property({ type: Object, attribute: false })
	public get filter(): (manifest: unknown) => boolean {
		return this.#filter;
	}
	public set filter(value: (manifest: unknown) => boolean) {
		if (value === this.#filter) return;
		this.#filter = value;
		this.#observeExtensions();
	}
	#filter: (manifest: unknown) => boolean = () => true;

	/**
	 * Properties to pass to all rendered extension elements.
	 * These properties are spread onto each extension element instance.
	 * Note: The extension's manifest is always passed automatically regardless of this setting.
	 * @example Pass entity context to extension elements
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="entityAction"
	 *     .elementProps=${{ entityType: 'document', unique: documentId }}>
	 * </umb-extension-with-api-slot>
	 * ```
	 */
	@property({ attribute: false })
	get elementProps(): Record<string, unknown> | undefined {
		return this.#elProps;
	}
	set elementProps(newVal: Record<string, unknown> | undefined) {
		// TODO, compare changes since last time. only reset the ones that changed. This might be better done by the controller is self:
		this.#elProps = newVal;
		if (this.#extensionsController) {
			this.#extensionsController.elementProperties = newVal;
		}
	}
	#elProps?: Record<string, unknown> = {};

	/**
	 * Constructor arguments to pass when instantiating the extension APIs.
	 * Can be an array of arguments or a function that receives the manifest and returns arguments.
	 * Note: The host controller is always prepended as the first argument automatically.
	 * @example Static array of arguments
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="workspaceAction"
	 *     .apiArgs=${[workspaceContext]}>
	 * </umb-extension-with-api-slot>
	 * ```
	 * @example Dynamic arguments based on manifest
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="entityAction"
	 *     .apiArgs=${(manifest) => [entityContext, manifest.meta.actionType]}>
	 * </umb-extension-with-api-slot>
	 * ```
	 */
	@property({ attribute: false })
	get apiArgs(): Array<unknown> | UmbApiConstructorArgumentsMethodType<unknown> | undefined {
		return this.#constructorArgs;
	}
	set apiArgs(newVal: Array<unknown> | UmbApiConstructorArgumentsMethodType<unknown> | undefined) {
		if (newVal === this.#constructorArgs) return;
		this.#constructorArgs = newVal;
		this.#observeExtensions();
	}
	#constructorArgs?: Array<unknown> | UmbApiConstructorArgumentsMethodType<unknown> = [];

	/**
	 * Properties to pass to all extension API instances after construction.
	 * These properties are spread onto each API instance.
	 * Note: The extension's manifest is always passed automatically regardless of this setting.
	 * @example Pass configuration to extension APIs
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="workspaceAction"
	 *     .apiProps=${{ saveOnClick: true, validateFirst: false }}>
	 * </umb-extension-with-api-slot>
	 * ```
	 */
	@property({ attribute: false })
	get apiProps(): Record<string, unknown> | undefined {
		return this.#apiProps;
	}
	set apiProps(newVal: Record<string, unknown> | undefined) {
		// TODO, compare changes since last time. only reset the ones that changed. This might be better done by the controller is self:
		this.#apiProps = newVal;
		if (this.#extensionsController) {
			this.#extensionsController.apiProperties = newVal;
		}
	}
	#apiProps?: Record<string, unknown> = {};

	/**
	 * Fallback element tag name to use when an extension manifest doesn't specify its own element.
	 * This allows extensions to rely on a default UI implementation while still being registered.
	 * @example Provide a default action element
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="entityAction"
	 *     default-element="umb-entity-action">
	 * </umb-extension-with-api-slot>
	 * ```
	 */
	@property({ type: String, attribute: 'default-element' })
	public defaultElement?: string;

	/**
	 * Fallback API loader to use when an extension manifest doesn't specify its own API.
	 * This allows extensions to rely on a default API implementation while still being registered.
	 * Can be a string path to the API module or an API loader function.
	 * @example Provide a default API
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="workspaceAction"
	 *     .defaultApi=${() => import('./default-workspace-action.api.js')}>
	 * </umb-extension-with-api-slot>
	 * ```
	 */
	@property({ type: String, attribute: 'default-api' })
	public defaultApi?: ApiLoaderProperty;

	/**
	 * Custom render function for controlling how each extension is rendered.
	 * When provided, this function is called for each permitted extension instead of the default rendering.
	 * The function receives the extension initializer (with `component`, `api`, and `manifest` properties) and the index,
	 * and should return a TemplateResult, HTMLElement, null, or nothing.
	 * @example Wrap extensions in custom markup
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="entityAction"
	 *     .renderMethod=${(ext, index) => html`
	 *         <div class="action-wrapper" data-index=${index}>
	 *             ${ext.component}
	 *         </div>
	 *     `}>
	 * </umb-extension-with-api-slot>
	 * ```
	 * @example Access API from render method
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="entityAction"
	 *     .renderMethod=${(ext, index) => {
	 *         console.log('API instance:', ext.api);
	 *         return ext.component;
	 *     }}>
	 * </umb-extension-with-api-slot>
	 * ```
	 */
	@property()
	public renderMethod?: (
		extension: UmbExtensionElementAndApiInitializer,
		index: number,
	) => TemplateResult | TemplateResult<1> | HTMLElement | null | undefined | typeof nothing;

	/**
	 * Render function called when no extensions are permitted.
	 * When provided, this function is called instead of rendering the default slot content.
	 * Useful for providing custom empty states or fallback UI.
	 * The function should return a TemplateResult, HTMLElement, null, or nothing.
	 * @example Custom empty state
	 * ```html
	 * <umb-extension-with-api-slot
	 *     type="entityAction"
	 *     .fallbackRenderMethod=${() => html`<div class="no-actions">No actions available</div>`}>
	 * </umb-extension-with-api-slot>
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
		this.#attached = false;
		this.#extensionsController?.destroy();
		this.#extensionsController = undefined;
		super.disconnectedCallback();
	}

	#observeExtensions(): void {
		// We want to be attached before we start observing extensions, cause first at this point we know that we got the right properties. [NL]
		if (!this.#attached) return;
		this.#extensionsController?.destroy();
		if (this.#type) {
			this.#extensionsController = new UmbExtensionsElementAndApiInitializer(
				this,
				umbExtensionsRegistry,
				this.#type,
				this.#constructorArgs,
				this.filter,
				(extensionControllers) => {
					this._permitted = extensionControllers;
				},
				undefined, // We can leave the alias to undefined, as we destroy this our selfs.
				this.defaultElement,
				this.defaultApi,
				{
					single: this.single,
				},
			);
			this.#extensionsController.apiProperties = this.#apiProps;
			this.#extensionsController.elementProperties = this.#elProps;
		}
	}

	override render() {
		return this._permitted
			? this._permitted.length > 0
				? repeat(this._permitted, (ext) => ext.alias, this.#renderExtension)
				: this.#renderNoting()
			: nothing;
	}

	#renderNoting() {
		return this.fallbackRenderMethod ? this.fallbackRenderMethod() : html`<slot></slot>`;
	}

	#renderExtension = (ext: UmbExtensionElementAndApiInitializer, i: number) => {
		return this.renderMethod ? this.renderMethod(ext, i) : ext.component;
	};

	static override styles = css`
		:host {
			display: contents;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-with-api-slot': UmbExtensionWithApiSlotElement;
	}
}
