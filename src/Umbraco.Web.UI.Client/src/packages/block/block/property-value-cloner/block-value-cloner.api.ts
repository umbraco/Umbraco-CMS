import type {
	UmbBlockDataValueModel,
	UmbBlockExposeModel,
	UmbBlockLayoutBaseModel,
	UmbBlockValueDataPropertiesBaseType,
} from '../types.js';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';
import type { UmbPropertyValueCloner } from '@umbraco-cms/backoffice/property';

export abstract class UmbBlockValueCloner<ValueType> implements UmbPropertyValueCloner<ValueType> {
	abstract cloneValue(incomingValue: UmbElementValueModel<ValueType>): Promise<UmbElementValueModel<ValueType>>;

	protected async _cloneBlock<ValueType extends UmbBlockValueDataPropertiesBaseType>(
		layoutEntry: UmbBlockLayoutBaseModel,
	): UmbBlockLayoutBaseModel {
		//... maybe have all content and settings and expose coming through this method or another apporach?
		return layoutEntry;
	}

	destroy(): void {}
}
