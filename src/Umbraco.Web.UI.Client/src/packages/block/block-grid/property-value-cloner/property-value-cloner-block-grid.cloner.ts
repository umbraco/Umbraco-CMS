import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../types.js';
import { UmbBlockPropertyValueCloner, type UmbBlockPropertyValueClonerArgs } from '@umbraco-cms/backoffice/block';

export class UmbBlockGridPropertyValueCloner extends UmbBlockPropertyValueCloner<
	UmbBlockGridValueModel,
	UmbBlockGridLayoutModel
> {
	constructor(args: UmbBlockPropertyValueClonerArgs) {
		super(UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS, args);
	}

	_cloneLayout(
		layouts: Array<UmbBlockGridLayoutModel> | undefined,
	): Promise<Array<UmbBlockGridLayoutModel> | undefined> | undefined {
		return layouts ? Promise.all(layouts.map(this.#cloneLayoutEntry)) : undefined;
	}

	#cloneLayoutEntry = async (layout: UmbBlockGridLayoutModel): Promise<UmbBlockGridLayoutModel> => {
		// Clone the specific layout entry:
		const entryClone = await this._cloneBlock(layout);
		if (entryClone.areas) {
			// And then clone the items of its areas:
			entryClone.areas = await Promise.all(
				entryClone.areas.map(async (area) => {
					return {
						...area,
						items: await Promise.all(area.items.map(this.#cloneLayoutEntry)),
					};
				}),
			);
		}
		return entryClone;
	};
}

export { UmbBlockGridPropertyValueCloner as api };
