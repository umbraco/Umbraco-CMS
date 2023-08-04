import { type ManifestTypes, umbExtensionsRegistry } from '../../extension-registry/index.js';
import { css, repeat, customElement, property, state, TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import {
	type UmbExtensionElementController,
	UmbExtensionsElementController,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-extension-slot
 * @description A element which renderers the extensions of a given type or types.
 * @slot default - slot for inserting additional things into this slot.
 * @export
 * @class UmbExtensionSlot
 * @extends {UmbLitElement}
 */

// TODO: Fire change event.
// TODO: Make property that reveals the amount of displayed/permitted extensions.
@customElement('umb-extension-slot')
export class UmbExtensionSlotElement extends UmbLitElement {
	#attached = false;
	#extensionsController?: UmbExtensionsElementController<ManifestTypes>;

	@state()
	private _permittedExts: Array<UmbExtensionElementController> = [];

	/**
	 * The type or types of extensions to render.
	 * @type {string | string[]}
	 * @memberof UmbExtensionSlot
	 * @example
	 * <umb-extension-slot type="umb-editor-footer"></umb-extension-slot>
	 * or multiple:
	 * <umb-extension-slot .type=${['umb-editor-footer','umb-editor-header']}></umb-extension-slot>
	 *
	 */
	@property({ type: String })
	public get type(): string | string[] | undefined {
		return this._type;
	}
	public set type(value: string | string[] | undefined) {
		if (value === this._type) return;
		this._type = value;
		if (this.#attached) {
			this._observeExtensions();
		}
	}
	private _type?: string | string[] | undefined;

	@property({ type: Object, attribute: false })
	public get filter(): (manifest: any) => boolean {
		return this._filter;
	}
	public set filter(value: (manifest: any) => boolean) {
		if (value === this._filter) return;
		this._filter = value;
		if (this.#attached) {
			this._observeExtensions();
		}
	}
	private _filter: (manifest: any) => boolean = () => true;

	private _props?: Record<string, any> = {};
	@property({ type: Object, attribute: false })
	get props() {
		return this._props;
	}
	set props(newVal) {
		// TODO, compare changes since last time. only reset the ones that changed. This might be better done by the controller is self:
		this._props = newVal;
		if (this.#extensionsController) {
			this.#extensionsController.properties = newVal;
		}
	}

	@property({ type: String, attribute: 'default-element' })
	public defaultElement = '';

	@property()
	public renderMethod?: (extension: UmbExtensionElementController) => TemplateResult | HTMLElement | null | undefined;

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();
		this.#attached = true;
	}

	private _observeExtensions() {
		this.#extensionsController?.destroy();
		if (this._type) {
			this.#extensionsController = new UmbExtensionsElementController(
				this,
				umbExtensionsRegistry,
				this._type,
				this.filter,
				(extensionControllers) => {
					this._permittedExts = extensionControllers;
				},
				this.defaultElement
			);
			this.#extensionsController.properties = this._props;
		}
	}

	render() {
		return repeat(
			this._permittedExts,
			(ext) => ext.alias,
			(ext) => (this.renderMethod ? this.renderMethod(ext) : ext.component)
		);
	}

	static styles = css`
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
