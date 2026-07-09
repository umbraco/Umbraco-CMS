import type { UmbSliderPropertyEditorUiValue } from './types.js';
import type { UmbPropertyValuePreset } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbSliderPropertyValuePreset
	implements UmbPropertyValuePreset<UmbSliderPropertyEditorUiValue, UmbPropertyEditorConfig>
{
	async processValue(value: undefined | UmbSliderPropertyEditorUiValue, config: UmbPropertyEditorConfig) {
		if (value !== undefined) {
			return value;
		}

		const enableRange = Boolean(config.find((x) => x.alias === 'enableRange')?.value ?? false);

		/*
		const min = Number(config.find((x) => x.alias === 'minVal')?.value ?? 0);
		const max = Number(config.find((x) => x.alias === 'maxVal')?.value ?? 100);
		const minVerified = isNaN(min) ? undefined : min;
		const maxVerified = isNaN(max) ? undefined : max;
		*/

		const step = Number(config.find((x) => x.alias === 'step')?.value ?? 0);
		const stepVerified = step > 0 ? step : 1;

		const initValueMin = Number(config.find((x) => x.alias === 'initVal1')?.value) || 0;
		const initValueMinVerified = isNaN(initValueMin) ? 0 : initValueMin;

		const initValueMax = Number(config.find((x) => x.alias === 'initVal2')?.value) || 0;
		const initValueMaxVerified = isNaN(initValueMax) ? initValueMinVerified + stepVerified : initValueMax;

		const initialState = enableRange
			? { from: initValueMinVerified, to: initValueMaxVerified }
			: { from: initValueMinVerified, to: initValueMinVerified };
		return initialState;
	}

	destroy(): void {}
}

export { UmbSliderPropertyValuePreset as api };
