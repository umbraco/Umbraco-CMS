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
 * @element umb-extension-with-api-slot
 * @description A element which renderers the extensions of a given type or types.
 * @slot default - slot for inserting additional things into this slot.
 * @class UmbExtensionSlot
 * @augments {UmbLitElement}
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

	@property({ type: Boolean })
	single?: boolean;

	/**
	 * The type or types of extensions to render.
	 * @type {string | string[]}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-with-api-slot type="my-extension-type"></umb-extension-with-api-slot>
	 * or multiple:
	 * <umb-extension-with-api-slot .type=${['my-extension-type','another-extension-type']}></umb-extension-with-api-slot>
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
	 * Filter method for extension manifests.
	 * This is an initial filter taking effect before conditions or overwrites, the extensions will still be filtered by the conditions defined in the manifest.
	 * @type {(manifest: any) => boolean}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-with-api-slot type="my-extension-type" .filter=${(ext) => ext.meta.anyPropToFilter === 'foo'}></umb-extension-with-api-slot>
	 */
	@property({ type: Object, attribute: false })
	public get filter(): (manifest: any) => boolean {
		return this.#filter;
	}
	public set filter(value: (manifest: any) => boolean) {
		if (value === this.#filter) return;
		this.#filter = value;
		this.#observeExtensions();
	}
	#filter: (manifest: any) => boolean = () => true;

	/**
	 * Properties to pass to the extensions elements.
	 * Notice: The individual manifest of the extension is parsed to each extension element no matter if this is set or not.
	 * @type {Record<string, any>}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-with-api-slot type="my-extension-type" .elementProps=${{foo: 'bar'}}></umb-extension-with-api-slot>
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
	 * constructor arguments to pass to the extensions apis.
	 * Notice: The host argument will be prepended as the first argument no matter if this is set or not.
	 * @type {Array<unknown>}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-with-api-slot type="my-extension-type" .apiArgs=${{foo: 'bar'}}></umb-extension-with-api-slot>
	 */
	@property({ attribute: false })
	get apiArgs(): Array<unknown> | UmbApiConstructorArgumentsMethodType<any> | undefined {
		return this.#constructorArgs;
	}
	set apiArgs(newVal: Array<unknown> | UmbApiConstructorArgumentsMethodType<any> | undefined) {
		if (newVal === this.#constructorArgs) return;
		this.#constructorArgs = newVal;
		this.#observeExtensions();
	}
	#constructorArgs?: Array<unknown> | UmbApiConstructorArgumentsMethodType<any> = [];

	/**
	 * Properties to pass to the extensions apis.
	 * Notice: The individual manifest of the extension is parsed to each extension api no matter if this is set or not.
	 * @type {Record<string, any>}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-with-api-slot type="my-extension-type" .apiProps=${{foo: 'bar'}}></umb-extension-with-api-slot>
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

	@property({ type: String, attribute: 'default-element' })
	public defaultElement?: string;

	@property({ type: String, attribute: 'default-api' })
	public defaultApi?: ApiLoaderProperty;

	@property()
	public renderMethod?: (
		extension: UmbExtensionElementAndApiInitializer,
		index: number,
	) => TemplateResult | TemplateResult<1> | HTMLElement | null | undefined | typeof nothing;

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
				: html`<slot></slot>`
			: nothing;
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
