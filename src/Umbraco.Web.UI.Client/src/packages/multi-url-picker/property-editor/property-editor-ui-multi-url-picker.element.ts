import type { UmbLinkPickerLink } from '../link-picker-modal/types.js';
import type { UmbInputMultiUrlElement } from '../components/input-multi-url/index.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

import '../components/input-multi-url/index.js';

/**
 * @element umb-property-editor-ui-multi-url-picker
 */
@customElement('umb-property-editor-ui-multi-url-picker')
export class UmbPropertyEditorUIMultiUrlPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Array })
	value: Array<UmbLinkPickerLink> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._hideAnchor = Boolean(config.getValueByAlias('hideAnchor'));
		this._min = this.#parseInt(config.getValueByAlias('minNumber'), 0);
		this._max = this.#parseInt(config.getValueByAlias('maxNumber'), Infinity);
		this._overlaySize = config.getValueByAlias<UUIModalSidebarSize>('overlaySize') ?? 'small';
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	#parseInt(value: unknown, fallback: number): number {
		const num = Number(value);
		return !Number.isNaN(num) && num > 0 ? num : fallback;
	}

	@state()
	private _overlaySize?: UUIModalSidebarSize;

	@state()
	private _hideAnchor?: boolean;

	@state()
	private _min = 0;

	@state()
	private _max = Infinity;

	@state()
	private _label?: string;

	@state()
	private _alias?: string;

	@state()
	private _variantId?: string;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this._label = context.getLabel();
			this.observe(context.alias, (alias) => (this._alias = alias));
			this.observe(context.variantId, (variantId) => (this._variantId = variantId?.toString() || 'invariant'));
		});
	}

	protected override firstUpdated() {
		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property '${this._label}' (Multi URL Picker) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputMultiUrlElement }) {
		this.value = event.target.urls;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-input-multi-url
				.alias=${this._alias}
				.max=${this._max}
				.min=${this._min}
				.overlaySize=${this._overlaySize}
				.urls=${this.value ?? []}
				.variantId=${this._variantId}
				?hide-anchor=${this._hideAnchor}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-multi-url>
		`;
	}
}

export default UmbPropertyEditorUIMultiUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multi-url-picker': UmbPropertyEditorUIMultiUrlPickerElement;
	}
}
