import { html, customElement, property, state, css, query } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUIRadioEvent } from '@umbraco-cms/backoffice/external/uui';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import type { UmbInputTimeZoneElement } from '@umbraco-cms/backoffice/components';

export interface UmbTimeZonePickerValue {
	mode: 'all' | 'local' | 'custom';
	timeZones: Array<string>;
}

/**
 * @element umb-property-editor-ui-time-zone-picker
 */
@customElement('umb-property-editor-ui-time-zone-picker')
export class UmbPropertyEditorUITimeZonePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _selectedTimeZones: Array<string> = [];

	@state()
	private _value: UmbTimeZonePickerValue | undefined;

	@property({ type: Object })
	public set value(value: UmbTimeZonePickerValue | undefined) {
		this._value = value;
	}

	public get value(): UmbTimeZonePickerValue | undefined {
		return this._value;
	}

	@property({ type: Boolean, reflect: true })
	readonly: boolean = false;

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@query('umb-input-time-zone')
	inputElem!: UmbInputTimeZoneElement;

	override connectedCallback() {
		super.connectedCallback();
		this._selectedTimeZones = this.value?.timeZones ?? [];
	}

	#onModeInput(event: UUIRadioEvent) {
		switch (event.target.value) {
			case 'none':
				this.value = undefined;
				break;
			case 'all':
				this.value = { mode: 'all', timeZones: [] };
				break;
			case 'local':
				this.value = { mode: 'local', timeZones: [] };
				break;
			case 'custom':
				this.value = {
					mode: 'custom',
					timeZones: Array.from(this.inputElem.value),
				};
				break;
			default:
				throw new Error(`Unknown value: ${event.target.value}`);
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputTimeZoneElement;
		const selectedOptions = target.value;

		this._selectedTimeZones = selectedOptions;
		if (this.value?.mode === 'custom') {
			this.value = { mode: this.value.mode, timeZones: selectedOptions };
		} else {
			this.value = { mode: this.value?.mode ?? 'all', timeZones: [] };
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-radio-group
				required
				?readonly=${this.readonly}
				@input=${this.#onModeInput}
				.value=${this.value?.mode ?? 'none'}>
				<uui-radio
					name="order"
					label=${this.localize.term('timeZonePicker_config_timeZones_disabled')}
					value="none"></uui-radio>
				<uui-radio
					name="order"
					label=${this.localize.term('timeZonePicker_config_timeZones_all')}
					value="all"></uui-radio>
				<uui-radio
					name="order"
					label=${this.localize.term('timeZonePicker_config_timeZones_local')}
					value="local"></uui-radio>
				<uui-radio
					name="order"
					label=${this.localize.term('timeZonePicker_config_timeZones_custom')}
					value="custom"></uui-radio>
			</uui-radio-group>
			<div class="timezone-picker" ?hidden=${this.value?.mode !== 'custom'}>
				<umb-form-validation-message>
					<umb-input-time-zone
						.value=${this._selectedTimeZones}
						?readonly=${this.readonly}
						?required=${this.value?.mode === 'custom'}
						@change=${this.#onChange}
						${umbBindToValidation(this)}>
					</umb-input-time-zone>
				</umb-form-validation-message>
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
