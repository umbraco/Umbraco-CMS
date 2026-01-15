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
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-property-editor-ui-tags
 */
@customElement('umb-property-editor-ui-tags')
export class UmbPropertyEditorUITagsElement
	extends UmbFormControlMixin<Array<string>, typeof UmbLitElement, undefined>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	public override set value(value: Array<string>) {
		super.value = value || [];
	}
	public override get value(): Array<string> {
		return super.value as string[];
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;
	@property({ type: Boolean })
	mandatory?: boolean;
	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

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

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-tags-input')!);
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
			?required=${!!this.mandatory}
			.requiredMessage=${this.mandatoryMessage}
			?readonly=${this.readonly}></umb-tags-input>`;
	}
}

export default UmbPropertyEditorUITagsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tags': UmbPropertyEditorUITagsElement;
	}
}
