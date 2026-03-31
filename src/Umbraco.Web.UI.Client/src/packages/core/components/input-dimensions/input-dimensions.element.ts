import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

export interface UmbInputDimensionsValue {
	width?: number;
	height?: number;
}

/**
 * A dimension input element with width/height fields and an aspect ratio lock.
 * @element umb-input-dimensions
 * @fires UmbChangeEvent - When width, height, or locked state changes.
 */
@customElement('umb-input-dimensions')
export class UmbInputDimensionsElement extends UmbFormControlMixin<
	UmbInputDimensionsValue | undefined,
	typeof UmbLitElement
>(UmbLitElement, undefined) {
	#ratio?: number;

	@property({ type: Number })
	public set width(value: number | undefined) {
		const oldValue = this.value?.width;
		if (value !== oldValue) {
			this.value = { ...this.value, width: value, height: this.value?.height };
			if (this.locked) this.#updateRatio();
		}
	}
	public get width(): number | undefined {
		return this.value?.width;
	}

	@property({ type: Number })
	public set height(value: number | undefined) {
		const oldValue = this.value?.height;
		if (value !== oldValue) {
			this.value = { ...this.value, width: this.value?.width, height: value };
			if (this.locked) this.#updateRatio();
		}
	}
	public get height(): number | undefined {
		return this.value?.height;
	}

	@property({ type: Boolean })
	locked = true;

	@property({ type: Boolean })
	disabled = false;

	@property({ type: Number, attribute: 'natural-width' })
	naturalWidth?: number;

	@property({ type: Number, attribute: 'natural-height' })
	naturalHeight?: number;

	override connectedCallback() {
		super.connectedCallback();
		this.#updateRatio();
	}

	#updateRatio() {
		const width = this.value?.width;
		const height = this.value?.height;
		if (width && height) {
			this.#ratio = width / height;
		}
	}

	#onWidthInput(e: UUIInputEvent) {
		const width = parseInt(e.target.value as string, 10);
		if (isNaN(width) || width <= 0) return;

		if (this.locked && this.#ratio) {
			this.value = { width, height: Math.round(width / this.#ratio) };
		} else {
			this.value = { ...this.value, width };
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onHeightInput(e: UUIInputEvent) {
		const height = parseInt(e.target.value as string, 10);
		if (isNaN(height) || height <= 0) return;

		if (this.locked && this.#ratio) {
			this.value = { width: Math.round(height * this.#ratio), height };
		} else {
			this.value = { ...this.value, height };
		}

		this.dispatchEvent(new UmbChangeEvent());
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
						.value=${this.value?.width?.toString() ?? ''}
						?disabled=${this.disabled}
						@input=${this.#onWidthInput}>
						<span class="extra" slot="append">px</span>
					</uui-input>
				</div>
				<uui-button
					compact
					label=${this.localize.term('general_constrainProportions')}
					title=${this.localize.term('general_constrainProportions')}
					look=${this.locked ? 'primary' : 'default'}
					?disabled=${this.disabled}
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
						.value=${this.value?.height?.toString() ?? ''}
						?disabled=${this.disabled}
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
						?disabled=${this.disabled}
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
