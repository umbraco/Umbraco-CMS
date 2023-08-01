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
		// TODO, compare changes since last time. only reset the ones that changed. This might be better done by the controller is self:
		this._props = newVal;
		if (this.#extensionsController) {
			this.#extensionsController.properties = newVal;
		}
	}

	@property({ type: String, attribute: 'default-element' })
	public defaultElement = '';

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();
	}

	private _observeExtensions() {
		this.#extensionsController?.destroy();
		if (this.type === 'treeItem') {
			console.log('observe', this.type, this.defaultElement);
		}
		this.#extensionsController = new UmbExtensionsElementController(
			this,
			this.type,
			this.filter,
			(extensionControllers) => {
				if (extensionControllers[0].manifest?.type === 'treeItem') {
					console.log('extensionControllers', extensionControllers);
				}
				this._permittedExts = extensionControllers;
			},
			this.defaultElement
		);
		this.#extensionsController.properties = this._props;
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
