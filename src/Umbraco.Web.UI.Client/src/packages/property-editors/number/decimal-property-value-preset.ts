import type { UmbDecimalPropertyEditorUiValue } from './types.js';
import type { UmbPropertyValuePreset } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbDecimalPropertyValuePreset
	implements UmbPropertyValuePreset<UmbDecimalPropertyEditorUiValue, UmbPropertyEditorConfig>
{
	async processValue(value: undefined | UmbDecimalPropertyEditorUiValue, config: UmbPropertyEditorConfig) {
		const defaultValue = this.#parseConfiguredNumber(config, 'defaultValue');
		if (defaultValue !== undefined) {
			return value !== undefined ? value : defaultValue;
		}

		const min = this.#parseConfiguredNumber(config, 'min') ?? 0;
		return value !== undefined ? value : min;
	}

	destroy(): void {}

	#parseConfiguredNumber(config: UmbPropertyEditorConfig, alias: string): number | undefined {
		const rawValue = config.find((x) => x.alias === alias)?.value;
		if (rawValue === undefined || rawValue === null || rawValue === '') {
			return undefined;
		}

		const parsedValue = Number(rawValue);
		return Number.isFinite(parsedValue) ? parsedValue : undefined;
	}
}

export { UmbDecimalPropertyValuePreset as api };
