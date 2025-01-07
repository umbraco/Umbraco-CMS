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
	 * <umb-extension-slot type="my-extension-type" .filter=${(ext) => ext.meta.anyPropToFilter === 'foo'}></umb-extension-slot>
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
	 * <umb-extension-slot type="my-extension-type" .props=${{foo: 'bar'}}></umb-extension-slot>
	 */
	@property({ type: Object, attribute: false })
	get props(): Record<string, unknown> | undefined {
		return this.#props;
	}
	set props(newVal: Record<string, unknown> | undefined) {
		this.#props = newVal;
		if (this.#extensionsController) {
			this.#extensionsController.properties = newVal;
		}
	}
	#props?: Record<string, unknown> = {};

	@property({ type: String, attribute: 'default-element' })
	public defaultElement?: string;

	@property({ attribute: false })
	public renderMethod?: (
		extension: UmbExtensionElementInitializer,
		index: number,
	) => TemplateResult | TemplateResult<1> | HTMLElement | null | undefined | typeof nothing;

	override connectedCallback(): void {
		super.connectedCallback();
		this.#attached = true;
		this.#observeExtensions();
	}
	override disconnectedCallback(): void {
		// _permitted is reset as the extensionsController fires a callback on destroy.
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
		return this._permitted
			? this._permitted.length > 0
				? repeat(this._permitted, (ext) => ext.alias, this.#renderExtension)
				: html`<slot></slot>`
			: nothing;
	}

	#renderExtension = (ext: UmbExtensionElementInitializer, i: number) => {
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
		'umb-extension-slot': UmbExtensionSlotElement;
	}
}
