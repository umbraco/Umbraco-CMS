import type { UmbTagsInputElement } from '../../components/tags-input/tags-input.element.js';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../../components/tags-input/tags-input.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-tags
 */
@customElement('umb-property-editor-ui-tags')
export class UmbPropertyEditorUITagsElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value: Array<string> = [];

	@property({ type: Array })
	public set value(value: Array<string>) {
		this._value = value || [];
	}
	public get value(): Array<string> {
		return this._value;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _group?: string;

	@state()
	private _storageType?: string;

	@state()
	private _culture?: string | null;
	//TODO: Use type from VariantID

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._group = config?.getValueByAlias('group');
		this._storageType = config?.getValueByAlias('storageType');
	}

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.variantId, (id) => {
				if (id && id.culture !== undefined) {
					this._culture = id.culture;
				}
			});
		});
	}

	#onChange(event: CustomEvent) {
		this.value = ((event.target as UmbTagsInputElement).value as string).split(',');
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-tags-input
			group=${ifDefined(this._group)}
			.culture=${this._culture}
			.items=${this.value}
			@change=${this.#onChange}
			?readonly=${this.readonly}></umb-tags-input>`;
	}
}

export default UmbPropertyEditorUITagsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tags': UmbPropertyEditorUITagsElement;
	}
}
