import { customElement, html, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import { umbBindToValidation, UmbValidationContext } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import {
	UMB_SUBMITTABLE_WORKSPACE_CONTEXT,
	UmbSubmittableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbInputMultipleTextStringElement } from '@umbraco-cms/backoffice/components';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-multiple-text-string
 */
@customElement('umb-property-editor-ui-multiple-text-string')
export class UmbPropertyEditorUIMultipleTextStringElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Array })
	value?: Array<string>;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._min = Number(config.getValueByAlias('min')) || 0;
		this._max = Number(config.getValueByAlias('max')) || Infinity;
	}

	/**
	 * Disables the Multiple Text String Property Editor UI
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Makes the Multiple Text String Property Editor UI readonly
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Makes the Multiple Text String Property Editor UI mandatory
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	required = false;

	@state()
	private _label?: string;

	@state()
	private _min = 0;

	@state()
	private _max = Infinity;

	@query('#input', true)
	protected _inputElement?: UmbInputMultipleTextStringElement;

	protected _validationContext = new UmbValidationContext(this);

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this._label = context.getLabel();
		});

		this.consumeContext(UMB_SUBMITTABLE_WORKSPACE_CONTEXT, (context) => {
			if (context instanceof UmbSubmittableWorkspaceContextBase) {
				context.addValidationContext(this._validationContext);
			}
		});
	}

	protected override firstUpdated() {
		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property '${this._label}' (Multiple Text String) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	#onChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UmbInputMultipleTextStringElement;
		this.value = target.items;
		this.dispatchEvent(new UmbChangeEvent());
	}

	// Prevent valid events from bubbling outside the message element
	#onValid(event: Event) {
		event.stopPropagation();
	}

	// Prevent invalid events from bubbling outside the message element
	#onInvalid(event: Event) {
		event.stopPropagation();
	}

	override render() {
		return html`
			<umb-form-validation-message id="validation-message" @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<umb-input-multiple-text-string
					id="input"
					max=${this._max}
					min=${this._min}
					.items=${this.value ?? []}
					?disabled=${this.disabled}
					?readonly=${this.readonly}
					?required=${this.required}
					@change=${this.#onChange}
					${umbBindToValidation(this)}>
				</umb-input-multiple-text-string>
			</umb-form-validation-message>
		`;
	}
}

export default UmbPropertyEditorUIMultipleTextStringElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multiple-text-string': UmbPropertyEditorUIMultipleTextStringElement;
	}
}
