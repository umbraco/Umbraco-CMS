import type { ManifestLocalization } from '../../extensions/localization.extension.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, query, state, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

interface UmbCultureInputOption {
	name: string;
	value: string;
}

@customElement('umb-ui-culture-input')
export class UmbUiCultureInputElement extends UUIFormControlMixin(UmbLitElement, '') {
	@state()
	private _options: Array<UmbCultureInputOption> = [];

	@query('uui-combobox')
	private _selectElement!: HTMLInputElement;

	@property({ type: String })
	override get value() {
		return super.value;
	}
	override set value(value: FormDataEntryValue | FormData) {
		if (typeof value === 'string') {
			const oldValue = super.value;
			super.value = value.toLowerCase();
			this.requestUpdate('value', oldValue);
		}
	}

	constructor() {
		super();
		this.#observeTranslations();
	}

	#observeTranslations() {
		this.observe(
			umbExtensionsRegistry.byType('localization'),
			(localizationManifests) => {
				this.#mapToOptions(localizationManifests);
			},
			'umbObserveLocalizationManifests',
		);
	}

	#mapToOptions(manifests: Array<ManifestLocalization>) {
		const options = manifests
			.filter((manifest) => !!manifest.meta.culture)
			.map((manifest) => {
				const culture = manifest.meta.culture.toLowerCase();
				return {
					name: manifest.name,
					value: culture,
				};
			});

		const distinct = [...new Map(options.map((item) => [item.value, item])).values()];

		this._options = distinct.sort((a, b) => a.value.localeCompare(b.value));
	}

	protected override getFormElement() {
		return this._selectElement;
	}

	#onCustomValidationChange(event: UUISelectEvent) {
		this.value = event.target.value.toString();
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-select
				style="margin-top: var(--uui-size-space-1)"
				@change=${this.#onCustomValidationChange}
				.options=${this._options.map((e) => ({
					name: e.name,
					value: e.value,
					selected: e.value == this.value,
				}))}></uui-select>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
			}
		`,
	];
}

export default UmbUiCultureInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ui-culture-input': UmbUiCultureInputElement;
	}
}
