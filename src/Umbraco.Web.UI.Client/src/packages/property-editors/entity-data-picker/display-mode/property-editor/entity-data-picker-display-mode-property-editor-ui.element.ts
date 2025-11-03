import type { ManifestEntityDataPickerDisplayMode } from '../extension/types.js';
import type { UmbEntityDataPickerDisplayModePropertyEditorValue } from './types.js';
import { customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUIComboboxElement } from '@umbraco-cms/backoffice/external/uui';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-entity-data-picker-display-mode-property-editor-ui')
export class UmbEntityDataPickerDisplayModePropertyEditorUIElement
	extends UmbFormControlMixin<UmbEntityDataPickerDisplayModePropertyEditorValue | undefined, typeof UmbLitElement>(
		UmbLitElement,
		undefined,
	)
	implements UmbPropertyEditorUiElement
{
	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _displayModes: Array<ManifestEntityDataPickerDisplayMode> = [];

	constructor() {
		super();
		this.#observeDisplayModes();
	}

	#observeDisplayModes() {
		this.observe(
			umbExtensionsRegistry.byType('entityDataPickerDisplayMode'),
			(extensions) => {
				this._displayModes = extensions;
			},
			'observeDisplayModes',
		);
	}

	override focus() {
		return this.shadowRoot?.querySelector<UUIComboboxElement>('uui-combobox')?.focus();
	}

	#onChange(event: CustomEvent & { target: UUIComboboxElement }) {
		const selected = event.target.value;

		// Ensure the value is of the correct type before setting it.
		if (typeof selected === 'string') {
			this.value = { ids: [selected] };
			this.dispatchEvent(new UmbChangeEvent());
		} else {
			throw new Error('Selected is not of type string. Cannot set property value.');
		}
	}

	override render() {
		return html` <uui-combobox value=${this.value?.ids[0] || ''} @change=${this.#onChange}>
			<uui-combobox-list
				>${repeat(
					this._displayModes ?? [],
					(property) => property.alias,
					(property) => this.#renderOption(property),
				)}</uui-combobox-list
			>
		</uui-combobox>`;
	}

	#renderOption(item: ManifestEntityDataPickerDisplayMode) {
		const label = item.meta?.label || item.name;
		const description = item.meta?.description;

		return html`
			<uui-combobox-list-option
				.displayValue=${label}
				style="display: flex; gap: 9px; align-items: center; padding: var(--uui-size-3)"
				.value=${item.alias}>
				<uui-icon name="icon-wrench"></uui-icon>
				<div style="display: flex; flex-direction: column">
					<b>${label}</b>
					${description ? html`<small>${description}</small>` : nothing}
				</div>
			</uui-combobox-list-option>
		`;
	}
}

export { UmbEntityDataPickerDisplayModePropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-data-picker-display-mode-property-editor-ui': UmbEntityDataPickerDisplayModePropertyEditorUIElement;
	}
}
