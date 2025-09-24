import { html, customElement, property, css, map, ref } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUIRadioEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbInputTimeZoneElement } from '@umbraco-cms/backoffice/components';

export interface UmbTimeZonePickerValue {
	mode: string;
	timeZones: Array<string>;
}

/**
 * @element umb-property-editor-ui-time-zone-picker
 */
@customElement('umb-property-editor-ui-time-zone-picker')
export class UmbPropertyEditorUITimeZonePickerElement
	extends UmbFormControlMixin<UmbTimeZonePickerValue, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	private _supportedModes = ['all', 'local', 'custom'];
	private _selectedTimeZones: Array<string> = [];

	override set value(value: UmbTimeZonePickerValue | undefined) {
		super.value = value;
		this._selectedTimeZones = value?.timeZones ?? [];
	}

	override get value(): UmbTimeZonePickerValue | undefined {
		return super.value;
	}

	@property({ type: Boolean, reflect: true })
	readonly: boolean = false;

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#inputTimeZone?: UmbInputTimeZoneElement;

	constructor() {
		super();

		this.addValidator(
			'valueMissing',
			() => UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => !this.value?.mode || this._supportedModes.indexOf(this.value.mode) === -1,
		);
	}

	#onModeInput(event: UUIRadioEvent) {
		if (!this._supportedModes.includes(event.target.value)) throw new Error(`Unknown mode: ${event.target.value}`);
		this.value = {
			mode: event.target.value,
			timeZones: this.#inputTimeZone?.value ? Array.from(this.#inputTimeZone?.value) : [],
		};
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputTimeZoneElement;
		const selectedOptions = target.value;

		if (this.value?.mode === 'custom') {
			this.value = { mode: this.value.mode, timeZones: selectedOptions };
		} else {
			this.value = { mode: this.value?.mode ?? 'all', timeZones: [] };
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	#inputTimeZoneRefChanged(input?: Element) {
		if (this.#inputTimeZone) {
			this.removeFormControlElement(this.#inputTimeZone);
		}
		this.#inputTimeZone = input as UmbInputTimeZoneElement | undefined;
		if (this.#inputTimeZone) {
			this.addFormControlElement(this.#inputTimeZone);
		}
	}

	override render() {
		return html`
			<uui-radio-group
				required
				?readonly=${this.readonly}
				@input=${this.#onModeInput}
				.value=${this.value?.mode ?? 'all'}>
				${map(
					this._supportedModes,
					(mode) =>
						html`<uui-radio
							name="order"
							label=${this.localize.term(`dateTimePicker_config_timeZones_${mode}`)}
							value="${mode}"></uui-radio>`,
				)}
			</uui-radio-group>
			<div class="timezone-picker" ?hidden=${this.value?.mode !== 'custom'}>
				<umb-input-time-zone
					.value=${this._selectedTimeZones}
					?readonly=${this.readonly}
					?required=${this.value?.mode === 'custom'}
					@change=${this.#onChange}
					${ref(this.#inputTimeZoneRefChanged)}>
				</umb-input-time-zone>
			</div>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: grid;
				gap: var(--uui-size-space-3);
			}

			.timezone-picker {
				display: flex;
				flex-direction: row;
				gap: var(--uui-size-space-6);
			}

			.timezone-picker[hidden] {
				display: none;
			}
		`,
	];
}

export default UmbPropertyEditorUITimeZonePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-time-zone-picker': UmbPropertyEditorUITimeZonePickerElement;
	}
}
