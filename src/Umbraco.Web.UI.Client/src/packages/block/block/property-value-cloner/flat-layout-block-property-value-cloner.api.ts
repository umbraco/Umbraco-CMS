import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '../types.js';
import { UmbBlockPropertyValueCloner } from './block-property-value-cloner.api.js';
export class UmbFlatLayoutBlockPropertyValueCloner<
	ValueType extends UmbBlockValueType = UmbBlockValueType,
> extends UmbBlockPropertyValueCloner<ValueType> {
	//
	_cloneLayout(
		layouts: Array<UmbBlockLayoutBaseModel> | undefined,
	): Promise<Array<UmbBlockLayoutBaseModel> | undefined> | undefined {
		return layouts ? Promise.all(layouts.map((layout) => this._cloneBlock(layout))) : undefined;
	}
}
