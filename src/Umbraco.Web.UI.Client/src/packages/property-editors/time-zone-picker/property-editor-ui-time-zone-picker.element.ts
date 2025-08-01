import { html, customElement, property, state, map, css, query } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUIRadioEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbTimeZonePickerValue } from '@umbraco-cms/backoffice/models';
import { getTimeZoneList, isEquivalentTimeZone } from '@umbraco-cms/backoffice/utils';

/**
 * @element umb-property-editor-ui-time-zone-picker
 */
@customElement('umb-property-editor-ui-time-zone-picker')
export class UmbPropertyEditorUITimeZonePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@state()
	private _value: UmbTimeZonePickerValue = { mode: 'all', timeZones: [] };

	@property({ type: Object })
	public set value(value: UmbTimeZonePickerValue | undefined) {
		this._value = value || { mode: 'all', timeZones: [] };
	}

	public get value(): UmbTimeZonePickerValue {
		return this._value;
	}

	@state()
	private _list: Array<Option> = [];

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@query('select')
	selectElem!: HTMLSelectElement;

	@state()
	private _displayTimeZonePicker = false;

	override firstUpdated() {
		if (!this.value) return;

		this._list = this.#getTimeZoneOptions();
		if (this.value.mode === 'custom') {
			this._displayTimeZonePicker = true;
		}
	}

	#getTimeZoneOptions(): Array<Option> {
		return getTimeZoneList().map((option) => ({
			value: option.value,
			name: option.name,
			selected:
				this._value.timeZones &&
				(this._value.timeZones.includes(option.value) ||
					this._value.timeZones.some((v) => isEquivalentTimeZone(v, option.value))),
		}));
	}

	#onInput(event: UUIRadioEvent) {
		switch (event.target.value) {
			case 'all':
				this.value = { mode: 'all', timeZones: [] };
				this._displayTimeZonePicker = false;
				break;
			case 'local':
				this.value = { mode: 'local', timeZones: [] };
				this._displayTimeZonePicker = false;
				break;
			case 'custom':
				this.value = {
					mode: 'custom',
					timeZones: Array.from(this.selectElem.selectedOptions).map((option) => option.value),
				};
				this._displayTimeZonePicker = true;
				break;
			default:
				throw new Error(`Unknown value: ${event.target.value}`);
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onSelectedChange(event: Event) {
		const select = event.target as HTMLSelectElement;
		this._value = { mode: 'custom', timeZones: Array.from(select.selectedOptions).map((option) => option.value) };
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-radio-group required @input=${this.#onInput} .value=${this.value.mode}>
				<uui-radio name="order" label="All" value="all"></uui-radio>
				<uui-radio name="order" label="Local" value="local"></uui-radio>
				<uui-radio name="order" label="Custom" value="custom"></uui-radio>
			</uui-radio-group>
			<div class="timezone-picker-container" ?hidden=${!this._displayTimeZonePicker}>
				<select id="timezone-select" multiple @change=${this.#onSelectedChange} size="15">
					${map(
						this._list,
						(item) => html`<option value=${item.value} ?selected=${item.selected}>${item.name}</option>`,
					)}
				</select>
				<div ?hidden=${!this.value.timeZones?.length}>
					<span>Selected time zones:</span>
					<ul>
						${map(this.value.timeZones, (item) => html`<li>${this._list.find((i) => i.value === item)?.name}</li>`)}
					</ul>
				</div>
			</div>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: grid;
				gap: var(--uui-size-space-3);
			}

			.timezone-picker-container {
				display: flex;
				flex-direction: row;
				gap: var(--uui-size-space-6);
			}

			.timezone-picker-container[hidden] {
				display: none;
			}

			#timezone-select {
				width: fit-content;
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
