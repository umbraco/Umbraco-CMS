import { UmbSliderPropertyValuePreset } from './slider-property-value-preset.js';
import type { UmbSliderPropertyEditorUiValue } from './types.js';

export class UmbRangeSliderPropertyValuePreset extends UmbSliderPropertyValuePreset {
	protected override resolveInitialState(initValueMin: number, initValueMax: number): UmbSliderPropertyEditorUiValue {
		return { from: initValueMin, to: initValueMax };
	}
}

export { UmbRangeSliderPropertyValuePreset as api };
