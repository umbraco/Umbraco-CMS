import { css, repeat, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementController, UmbExtensionsElementController } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

export type InitializedExtension = { alias: string; weight: number; component: HTMLElement | null };

/**
 * @element umb-extension-slot
 * @description
 * @slot default - slot for inserting additional things into this slot.
 * @export
 * @class UmbExtensionSlot
 * @extends {UmbLitElement}
 */

// TODO: Fire change event.
// TODO: Make property that reveals the amount of displayed/permitted extensions.
@customElement('umb-extension-slot')
export class UmbExtensionSlotElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementController;

	@state()
	private _extensions: Array<UmbExtensionElementController> = [];

	@state()
	private _permittedExts: Array<UmbExtensionElementController> = [];

	@property({ type: String })
	public type = '';

	@property({ type: Object, attribute: false })
	public filter: (manifest: any) => boolean = () => true;

	private _props?: Record<string, any> = {};
	@property({ type: Object, attribute: false })
	get props() {
		return this._props;
	}
	set props(newVal) {
		this._props = newVal;
		this._extensions.forEach((ext) => (ext.properties = this._props));
	}

	@property({ type: String, attribute: 'default-element' })
	public defaultElement = '';

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();
	}

	private _observeExtensions() {
		this.#extensionsController?.destroy();
		console.log('observe', this.type, this.defaultElement);
		this.#extensionsController = new UmbExtensionsElementController(
			this,
			this.type,
			this.filter,
			(extensionControllers) => {
				if (extensionControllers[0].manifest?.type === 'menuItem') {
					console.log('extensionControllers', extensionControllers);
				}
				this._permittedExts = extensionControllers;
			},
			this.defaultElement
		);
	}

	render() {
		return repeat(
			this._permittedExts,
			(ext) => ext.alias,
			(ext) => ext.component
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
