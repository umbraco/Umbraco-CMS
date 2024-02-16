import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context.js';
import { UMB_BLOCK_GRID_ENTRIES_CONTEXT } from './block-grid-entries.context-token.js';
import {
	UmbBlockEntryContext,
	type UmbBlockGridTypeModel,
	type UmbBlockGridLayoutModel,
} from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState, appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import { combineLatest } from '@umbraco-cms/backoffice/external/rxjs';

// Utils:
// TODO: Move these methods into their own files:

function getInterpolatedIndexOfPositionInWeightMap(target: number, weights: Array<number>) {
	const map = [0];
	weights.reduce((a, b, i) => {
		return (map[i + 1] = a + b);
	}, 0);
	const foundValue = map.reduce((a, b) => {
		const aDiff = Math.abs(a - target);
		const bDiff = Math.abs(b - target);

		if (aDiff === bDiff) {
			return a < b ? a : b;
		} else {
			return bDiff < aDiff ? b : a;
		}
	});
	const foundIndex = map.indexOf(foundValue);
	const targetDiff = target - foundValue;
	let interpolatedIndex = foundIndex;
	if (targetDiff < 0 && foundIndex === 0) {
		// Don't adjust.
	} else if (targetDiff > 0 && foundIndex === map.length - 1) {
		// Don't adjust.
	} else {
		const foundInterpolationWeight = weights[targetDiff >= 0 ? foundIndex : foundIndex - 1];
		interpolatedIndex += foundInterpolationWeight === 0 ? interpolatedIndex : targetDiff / foundInterpolationWeight;
	}
	return interpolatedIndex;
}

function getAccumulatedValueOfIndex(index: number, weights: Array<number>) {
	const len = Math.min(index, weights.length);
	let i = 0,
		calc = 0;
	while (i < len) {
		calc += weights[i++];
	}
	return calc;
}

function closestColumnSpanOption(target: number, map: Array<number>, max: number) {
	if (map.length > 0) {
		const result = map.reduce((a, b) => {
			if (a > max) {
				return b;
			}
			const aDiff = Math.abs(a - target);
			const bDiff = Math.abs(b - target);

			if (aDiff === bDiff) {
				return a < b ? a : b;
			} else {
				return bDiff < aDiff ? b : a;
			}
		});
		if (result) {
			return result;
		}
	}
	return;
}

export class UmbBlockGridEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_GRID_MANAGER_CONTEXT,
	typeof UMB_BLOCK_GRID_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_GRID_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_GRID_ENTRIES_CONTEXT.TYPE,
	UmbBlockGridTypeModel,
	UmbBlockGridLayoutModel
> {
	//
	readonly columnSpan = this._layout.asObservablePart((x) => x?.columnSpan);
	readonly rowSpan = this._layout.asObservablePart((x) => x?.rowSpan ?? 1);
	readonly columnSpanOptions = this._blockType.asObservablePart((x) => x?.columnSpanOptions ?? []);
	readonly areaGridColumns = this._blockType.asObservablePart((x) => x?.areaGridColumns);
	readonly areas = this._blockType.asObservablePart((x) => x?.areas ?? []);
	readonly minMaxRowSpan = this._blockType.asObservablePart((x) => [x?.rowMinSpan ?? 0, x?.rowMaxSpan ?? Infinity]);

	#relevantColumnSpanOptions = new UmbArrayState<number>([], (x) => x);
	readonly relevantColumnSpanOptions = this.#relevantColumnSpanOptions.asObservable();

	#canScale = new UmbBooleanState(false);
	readonly canScale = this.#canScale.asObservable();

	readonly showContentEdit = this._blockType.asObservablePart((x) => !x?.forceHideContentEditorInOverlay);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_MANAGER_CONTEXT, UMB_BLOCK_GRID_ENTRIES_CONTEXT);

		this.observe(this.relevantColumnSpanOptions, (relevantColumnSpanOptions) => {
			if (relevantColumnSpanOptions.length === 0) {
				// Reset to the layoutColumns.
			} else {
				// Correct columnSpan so it fits.
			}
			console.log('relevantColumnSpanOptions', relevantColumnSpanOptions);
		});
	}

	layoutsOfArea(areaKey: string) {
		return this._layout.asObservablePart((x) => x?.areas.find((x) => x.key === areaKey)?.items ?? []);
	}

	areaType(areaKey: string) {
		return this._blockType.asObservablePart((x) => x?.areas?.find((x) => x.key === areaKey));
	}

	setLayoutsOfArea(areaKey: string, layouts: UmbBlockGridLayoutModel[]) {
		const frozenValue = this._layout.value;
		if (!frozenValue) return;
		const areas = appendToFrozenArray(
			frozenValue?.areas,
			{
				key: areaKey,
				items: layouts,
			},
			(x) => x.key,
		);
		this._layout.update({ areas });
	}

	setColumnSpan(columnSpan: number) {
		if (!this._entries) return;
		const layoutColumns = this._entries.getLayoutColumns();
		if (!layoutColumns) return;
		const oldColumnSpan = this._layout.getValue()?.columnSpan;
		if (!oldColumnSpan) {
			// Some fallback solution, to reset it so something that makes sense.
			return;
		}

		columnSpan = Math.max(1, Math.min(columnSpan, layoutColumns));

		const columnSpanOptions = this.#relevantColumnSpanOptions.getValue();
		if (columnSpanOptions.length > 0) {
			columnSpan = closestColumnSpanOption(columnSpan, columnSpanOptions, layoutColumns) ?? layoutColumns;
		}
		this._layout.update({ columnSpan });
	}

	_gotManager() {
		if (this._manager) {
			/*this.observe(
				this._manager.inlineEditingMode,
				(inlineEditingMode) => {
					this.#inlineEditingMode.setValue(inlineEditingMode);
				},
				'observeInlineEditingMode',
			);*/
		} else {
			//this.removeControllerByAlias('observeInlineEditingMode');
		}
	}

	_gotEntries() {
		if (!this._entries) return;

		this.observe(
			combineLatest([this.minMaxRowSpan, this.columnSpanOptions, this._entries.layoutColumns]),
			([minMaxRowSpan, columnSpanOptions, layoutColumns]) => {
				if (!layoutColumns) return;
				console.log('calc', columnSpanOptions, layoutColumns);
				const relevantColumnSpanOptions = columnSpanOptions
					? columnSpanOptions
							.filter((x) => x.columnSpan <= layoutColumns)
							.map((x) => x.columnSpan)
							.sort((a, b) => (a > b ? 1 : b > a ? -1 : 0))
					: [];
				this.#relevantColumnSpanOptions.setValue(relevantColumnSpanOptions);
				const hasRelevantColumnSpanOptions = relevantColumnSpanOptions.length > 1;
				const hasRowSpanOptions = minMaxRowSpan[0] !== minMaxRowSpan[1];
				const canScale = hasRelevantColumnSpanOptions || hasRowSpanOptions;

				this.#canScale.setValue(canScale);
			},
			'observeScaleOptions',
		);
	}
}
