import { customElement, html, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUIColorPickerChangeEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-eye-dropper')
export class UmbInputEyeDropperElement extends UUIFormControlMixin(UmbLitElement, '') {
	protected override getFormElement() {
		return undefined;
	}

	#onChange(e: UUIColorPickerChangeEvent) {
		e.stopPropagation();
		this.value = e.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	@property({ type: Boolean })
	opacity = false;

	@property({ type: Boolean })
	showPalette = false;

	@property({ type: Array })
	swatches?: string[];

	// HACK: Since `uui-color-picker` doesn't have an option to hide the swatches, we had to get creative.
	// Based on UUI v1.8.0-rc3, the value of `swatches` must be a falsey value to hide them.
	// https://github.com/umbraco/Umbraco.UI/blob/v1.8.0-rc.3/packages/uui-color-picker/lib/uui-color-picker.element.ts#L517
	// However, the object-type for `swatches` is a `string[]` (non-nullable).
	// https://github.com/umbraco/Umbraco.UI/blob/v1.8.0-rc.3/packages/uui-color-picker/lib/uui-color-picker.element.ts#L157
	// To do this, we must omit the `.swatches` attribute, otherwise the default swatches can't be used.
	// So, we've use a `when()` render both configurations. [LK]

	override render() {
		const swatches = this.showPalette ? this.swatches : undefined;
		return when(
			this.showPalette && !swatches,
			() => html`
				<uui-color-picker
					label="Eye dropper"
					.opacity=${this.opacity}
					.value=${this.value as string}
					@change=${this.#onChange}>
				</uui-color-picker>
			`,
			() => html`
				<uui-color-picker
					label="Eye dropper"
					.opacity=${this.opacity}
					.swatches=${swatches!}
					.value=${this.value as string}
					@change=${this.#onChange}>
				</uui-color-picker>
			`,
		);
	}
}

export default UmbInputEyeDropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-eye-dropper': UmbInputEyeDropperElement;
	}
}
