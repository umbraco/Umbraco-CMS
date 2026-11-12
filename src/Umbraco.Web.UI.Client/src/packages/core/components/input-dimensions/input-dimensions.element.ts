import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

export interface UmbInputDimensionsValue {
	width?: number;
	height?: number;
}

/**
 * A form control for a width and height, with an aspect ratio lock. Its value is a single `{ width, height }` object.
 * @element umb-input-dimensions
 * @fires UmbChangeEvent - When width, height, or locked state changes.
 */
@customElement('umb-input-dimensions')
export class UmbInputDimensionsElement extends UmbFormControlMixin<
	UmbInputDimensionsValue | undefined,
	typeof UmbLitElement
>(UmbLitElement, undefined) {
	#ratio?: number;

	constructor() {
		super();
		this.addValidator(
			'valueMissing',
			() => this.requiredMessage,
			() => this.required && (!this.value?.width || !this.value?.height),
		);
	}

	@property({ attribute: false })
	public override set value(newValue: UmbInputDimensionsValue | undefined) {
		super.value = newValue;
		this.#updateRatio();
	}
	public override get value(): UmbInputDimensionsValue | undefined {
		return super.value;
	}

	@property({ type: Number })
	public set width(value: number | undefined) {
		if (value !== this.value?.width) {
			this.value = { ...this.value, width: value };
		}
	}
	public get width(): number | undefined {
		return this.value?.width;
	}

	@property({ type: Number })
	public set height(value: number | undefined) {
		if (value !== this.value?.height) {
			this.value = { ...this.value, height: value };
		}
	}
	public get height(): number | undefined {
		return this.value?.height;
	}

	/**
	 * Keeps the width and height proportional when one of them is changed.
	 * @type {boolean}
	 * @default true
	 */
	@property({ type: Boolean })
	locked = true;

	/**
	 * Disables the inputs and buttons.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Makes the inputs readonly and disables the buttons, so the value cannot be changed.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Requires both a width and a height.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean })
	required = false;

	/**
	 * Message shown when the control is required and a width or height is missing.
	 * @type {string}
	 */
	@property({ type: String })
	requiredMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	/**
	 * The natural width, which the reset button restores together with the natural height.
	 * @type {number}
	 */
	@property({ type: Number, attribute: 'natural-width' })
	naturalWidth?: number;

	/**
	 * The natural height, which the reset button restores together with the natural width.
	 * @type {number}
	 */
	@property({ type: Number, attribute: 'natural-height' })
	naturalHeight?: number;

	override connectedCallback() {
		super.connectedCallback();
		this.#updateRatio();
	}

	protected override firstUpdated() {
		this.shadowRoot?.querySelectorAll<UUIInputElement>('uui-input').forEach((x) => this.addFormControlElement(x));
	}

	protected getInitialValue(): UmbInputDimensionsValue | undefined {
		return undefined;
	}

	override focus() {
		return this.shadowRoot?.querySelector<UUIInputElement>('uui-input')?.focus();
	}

	#updateRatio() {
		const width = this.value?.width;
		const height = this.value?.height;
		if (width && height) {
			this.#ratio = width / height;
		}
	}

	#setValue(value: UmbInputDimensionsValue) {
		super.value = value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onWidthInput(e: UUIInputEvent) {
		const input = e.target.value as string;
		if (input === '') {
			this.#setValue({ ...this.value, width: undefined });
			return;
		}

		const width = parseInt(input, 10);
		if (isNaN(width) || width <= 0) return;

		this.#setValue(
			this.locked && this.#ratio ? { width, height: Math.round(width / this.#ratio) } : { ...this.value, width },
		);
	}

	#onHeightInput(e: UUIInputEvent) {
		const input = e.target.value as string;
		if (input === '') {
			this.#setValue({ ...this.value, height: undefined });
			return;
		}

		const height = parseInt(input, 10);
		if (isNaN(height) || height <= 0) return;

		this.#setValue(
			this.locked && this.#ratio ? { width: Math.round(height * this.#ratio), height } : { ...this.value, height },
		);
	}

	#onToggleLock() {
		this.locked = !this.locked;

		if (this.locked) {
			this.#updateRatio();
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onReset() {
		if (!this.naturalWidth || !this.naturalHeight) return;

		this.value = { width: this.naturalWidth, height: this.naturalHeight };
		this.#ratio = this.naturalWidth / this.naturalHeight;
		this.locked = true;

		this.dispatchEvent(new UmbChangeEvent());
	}

	get #hasNaturalDimensions(): boolean {
		return this.naturalWidth != null && this.naturalHeight != null && this.naturalWidth > 0 && this.naturalHeight > 0;
	}

	get #isNatural(): boolean {
		return this.value?.width === this.naturalWidth && this.value?.height === this.naturalHeight;
	}

	override render() {
		return html`
			<div id="dimensions">
				<div class="dimension-field">
					<uui-label for="width">${this.localize.term('general_width')}</uui-label>
					<uui-input
						id="width"
						type="number"
						label=${this.localize.term('general_width')}
						placeholder=${this.localize.term('general_width')}
						min="1"
						?required=${this.required}
						.value=${this.value?.width?.toString() ?? ''}
						?disabled=${this.disabled}
						?readonly=${this.readonly}
						@input=${this.#onWidthInput}>
						<span class="extra" slot="append">px</span>
					</uui-input>
				</div>
				<uui-button
					compact
					label=${this.localize.term('general_constrainProportions')}
					title=${this.localize.term('general_constrainProportions')}
					look=${this.locked ? 'secondary' : 'default'}
					?disabled=${this.disabled || this.readonly}
					@click=${this.#onToggleLock}>
					<uui-icon name=${this.locked ? 'icon-lock' : 'icon-unlocked'}></uui-icon>
				</uui-button>
				<div class="dimension-field">
					<uui-label for="height">${this.localize.term('general_height')}</uui-label>
					<uui-input
						id="height"
						type="number"
						label=${this.localize.term('general_height')}
						placeholder=${this.localize.term('general_height')}
						min="1"
						?required=${this.required}
						.value=${this.value?.height?.toString() ?? ''}
						?disabled=${this.disabled}
						?readonly=${this.readonly}
						@input=${this.#onHeightInput}>
						<span class="extra" slot="append">px</span>
					</uui-input>
				</div>
			</div>
			${this.#hasNaturalDimensions && !this.#isNatural
				? html`<uui-button
						id="reset"
						compact
						label=${this.localize.term('general_reset')}
						?disabled=${this.disabled || this.readonly}
						@click=${this.#onReset}></uui-button>`
				: nothing}
		`;
	}

	static override styles = css`
		#dimensions {
			display: flex;
			align-items: end;
			gap: var(--uui-size-space-3);

			.dimension-field {
				flex: 1;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);

				uui-input {
					margin-bottom: 0;
				}

				.extra {
					user-select: none;
					height: 100%;
					padding: 0 var(--uui-size-3);
					border-left: 1px solid var(--uui-input-border-color, var(--uui-color-border));
					background: var(--uui-color-background);
					color: var(--uui-color-text);
					display: flex;
					justify-content: center;
					align-items: center;
				}
			}
		}

		#reset {
			margin-bottom: var(--uui-size-space-1);
		}
	`;
}

export default UmbInputDimensionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-dimensions': UmbInputDimensionsElement;
	}
}
