import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UMB_DEFAULT_LOCALIZATION_CULTURE } from '@umbraco-cms/backoffice/localization-api';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

interface UmbCultureInputOption {
	name: string;
	value: string;
}

@customElement('umb-ui-culture-input')
export class UmbUiCultureInputElement extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(
	UmbLitElement,
) {
	#invalidCulture?: string;
	#invalidBaseCulture?: string;

	@state()
	private _options: Array<UmbCultureInputOption> = [];

	@property({ type: String })
	override set value(value: string | undefined) {
		if (value && typeof value === 'string') {
			const oldValue = super.value;
			super.value = value.toLowerCase();
			this.requestUpdate('value', oldValue);
		}
	}
	override get value(): string | undefined {
		return super.value;
	}

	constructor() {
		super();

		this.observe(
			umbExtensionsRegistry.byType('localization'),
			(manifests) => {
				const options = manifests
					.filter((manifest) => !!manifest.meta.culture)
					.map((manifest) => {
						const culture = manifest.meta.culture.toLowerCase();
						return {
							name: this.localize.term(`uiCulture_${culture}`),
							value: culture,
						};
					});

				const distinct = [...new Map(options.map((item) => [item.value, item])).values()];

				this._options = distinct.sort((a, b) => a.name.localeCompare(b.name));
			},
			'umbObserveLocalizationManifests',
		);

		this.addValidator(
			'customError',
			() => this.localize.term('user_languageNotFound', this.#invalidCulture, this.value),
			() => !!this.#invalidCulture && !this.#invalidBaseCulture,
		);

		this.addValidator(
			'customError',
			() => this.localize.term('user_languageNotFoundFallback', this.#invalidCulture, this.#invalidBaseCulture),
			() => !!this.#invalidCulture && !!this.#invalidBaseCulture,
		);
	}

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);

		if (this.value) {
			// Check if the culture can be found.
			const found = this._options.find((option) => option.value === this.value);
			if (!found) {
				this.#invalidCulture = this.value;

				// if not found, check for the base culture
				const locale = new Intl.Locale(this.value);
				if (locale.language) {
					const foundBase = this._options.find((option) => option.value === locale.language);
					if (foundBase) {
						this.value = locale.language;
					} else {
						// if the base culture is not found, set the value to "en"
						this.#invalidBaseCulture = locale.language;
						this.value = UMB_DEFAULT_LOCALIZATION_CULTURE;
					}
				} else {
					// if the base culture is not found, set the value to "en"
					this.value = UMB_DEFAULT_LOCALIZATION_CULTURE;
				}
			}
		}

		this.addFormControlElement(this.shadowRoot!.querySelector('uui-select')!);
		this.checkValidity();
	}

	#onCustomValidationChange(event: UUISelectEvent) {
		this.#invalidCulture = undefined;
		this.#invalidBaseCulture = undefined;
		this.value = event.target.value.toString();
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-select
				.options=${this._options.map((e) => ({ ...e, selected: e.value == this.value }))}
				@change=${this.#onCustomValidationChange}>
			</uui-select>
		`;
	}
}

export default UmbUiCultureInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ui-culture-input': UmbUiCultureInputElement;
	}
}
