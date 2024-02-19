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

	#runtimeGridColumns: Array<number> = [];
	#runtimeGridRows: Array<number> = [];
	#lockedGridRows = 0;

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
		/*
		const oldColumnSpan = this._layout.getValue()?.columnSpan;
		if (!oldColumnSpan) {
			// Some fallback solution, to reset it so something that makes sense.
			return;
		}
		*/

		columnSpan = Math.max(1, Math.min(columnSpan, layoutColumns));

		/*
		const columnSpanOptions = this.#relevantColumnSpanOptions.getValue();
		if (columnSpanOptions.length > 0) {
			columnSpan = closestColumnSpanOption(columnSpan, columnSpanOptions, layoutColumns) ?? layoutColumns;
		}*/
		this._layout.update({ columnSpan });
	}
	getColumnSpan() {
		return this._layout.getValue()?.columnSpan;
	}

	setRowSpan(rowSpan: number) {
		const blockType = this._blockType.getValue();
		if (!blockType) return;
		rowSpan = Math.max(blockType.rowMinSpan, Math.min(rowSpan, blockType.rowMaxSpan));
		this._layout.update({ rowSpan });
	}

	getRowSpan() {
		return this._layout.getValue()?.rowSpan;
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

	// Scaling feature:

	#updateGridData(
		layoutContainer: HTMLElement,
		layoutContainerRect: DOMRect,
		layoutItemRect: DOMRect,
		updateRowTemplate: boolean,
	) {
		if (!this._entries) return;

		const computedStyles = window.getComputedStyle(layoutContainer);

		const columnGap = Number(computedStyles.columnGap.split('px')[0]) || 0;
		const rowGap = Number(computedStyles.rowGap.split('px')[0]) || 0;

		let gridColumns = computedStyles.gridTemplateColumns
			.trim()
			.split('px')
			.map((x) => Number(x));
		let gridRows = computedStyles.gridTemplateRows
			.trim()
			.split('px')
			.map((x) => Number(x));

		// remove empties:
		gridColumns = gridColumns.filter((n) => n > 0);
		gridRows = gridRows.filter((n) => n > 0);

		// We use this code to lock the templateRows, while scaling. otherwise scaling Rows is too crazy.
		if (updateRowTemplate || gridRows.length > this.#lockedGridRows) {
			this.#lockedGridRows = gridRows.length;
			layoutContainer.style.gridTemplateRows = computedStyles.gridTemplateRows;
		}

		// add gaps:
		const gridColumnsLen = gridColumns.length;
		gridColumns = gridColumns.map((n, i) => (gridColumnsLen === i ? n : n + columnGap));
		const gridRowsLen = gridRows.length;
		gridRows = gridRows.map((n, i) => (gridRowsLen === i ? n : n + rowGap));

		// ensure all columns are there.
		// This will also ensure handling non-css-grid mode,
		// use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
		let amountOfColumnsInWeightMap = gridColumns.length;
		const layoutColumns = this._entries.getLayoutColumns() ?? 0;
		const amountOfUnknownColumns = layoutColumns - amountOfColumnsInWeightMap;
		if (amountOfUnknownColumns > 0) {
			const accumulatedValue = getAccumulatedValueOfIndex(amountOfColumnsInWeightMap, gridColumns) || 0;
			const missingColumnWidth = (layoutContainerRect.width - accumulatedValue) / amountOfUnknownColumns;
			while (amountOfColumnsInWeightMap++ < layoutColumns) {
				gridColumns.push(missingColumnWidth);
			}
		}

		// Handle non css grid mode for Rows:
		// use item height divided by rowSpan to identify row heights.
		if (gridRows.length === 0) {
			// Push its own height twice, to give something to scale with.
			gridRows.push(layoutItemRect.top - layoutContainerRect.top);

			let i = 0;
			const itemSingleRowHeight = layoutItemRect.height;
			while (i++ < (this.getRowSpan() ?? 0)) {
				gridRows.push(itemSingleRowHeight);
			}
		}

		// add a few extra rows, so there is something to extend too.
		// Add extra options for the ability to extend beyond current content:
		gridRows.push(50);
		gridRows.push(50);
		gridRows.push(50);
		gridRows.push(50);
		gridRows.push(50);

		this.#runtimeGridColumns = gridColumns;
		this.#runtimeGridRows = gridRows;
	}

	// TODO: Rename to calc something.
	#getNewSpans(startX: number, startY: number, endX: number, endY: number) {
		const layoutColumns = this._entries?.getLayoutColumns();
		if (!layoutColumns) return;

		const blockStartCol = Math.round(getInterpolatedIndexOfPositionInWeightMap(startX, this.#runtimeGridColumns));
		const blockStartRow = Math.round(getInterpolatedIndexOfPositionInWeightMap(startY, this.#runtimeGridRows));
		const blockEndCol = getInterpolatedIndexOfPositionInWeightMap(endX, this.#runtimeGridColumns);
		const blockEndRow = getInterpolatedIndexOfPositionInWeightMap(endY, this.#runtimeGridRows);

		let newColumnSpan = Math.max(blockEndCol - blockStartCol, 1);

		// Find nearest allowed Column:
		const bestColumnSpanOption = closestColumnSpanOption(
			newColumnSpan,
			this.#relevantColumnSpanOptions.getValue(),
			layoutColumns - blockStartCol,
		);
		newColumnSpan = bestColumnSpanOption ?? layoutColumns;

		let newRowSpan = Math.round(Math.max(blockEndRow - blockStartRow, this._blockType.getValue()?.rowMinSpan || 1));
		const rowMaxSpan = this._blockType.getValue()?.rowMaxSpan;
		if (rowMaxSpan != null) {
			newRowSpan = Math.min(newRowSpan, rowMaxSpan);
		}

		return { columnSpan: newColumnSpan, rowSpan: newRowSpan, startCol: blockStartCol, startRow: blockStartRow };
	}

	public onScaleMouseDown(event: MouseEvent) {
		const layoutContainer = this._entries?.getLayoutContainerElement() as HTMLElement;
		if (!layoutContainer) {
			return;
		}
		event.preventDefault();

		console.log('onScaleMouseDown');

		//this.#isScaleMode = true;

		window.addEventListener('mousemove', this.onScaleMouseMove);
		window.addEventListener('mouseup', this.onScaleMouseUp);
		window.addEventListener('mouseleave', this.onScaleMouseUp);

		const layoutItemRect = this.getHostElement().getBoundingClientRect();
		this.#updateGridData(layoutContainer, layoutContainer.getBoundingClientRect(), layoutItemRect, true);

		/*
		scaleBoxBackdropEl = document.createElement('div');
		scaleBoxBackdropEl.className = 'umb-block-grid__scalebox-backdrop';
		layoutContainer.appendChild(scaleBoxBackdropEl);
		*/
	}

	onScaleMouseMove = (e: MouseEvent) => {
		const layoutContainer = this._entries?.getLayoutContainerElement() as HTMLElement;
		if (!layoutContainer) {
			return;
		}
		const layoutContainerRect = layoutContainer.getBoundingClientRect();
		const layoutItemRect = this.getHostElement().getBoundingClientRect();

		const startX = layoutItemRect.left - layoutContainerRect.left;
		const startY = layoutItemRect.top - layoutContainerRect.top;
		const endX = e.clientX - layoutContainerRect.left;
		const endY = e.clientY - layoutContainerRect.top;

		const newSpans = this.#getNewSpans(startX, startY, endX, endY);
		if (!newSpans) return;

		const updateRowTemplate = this.getColumnSpan() !== newSpans.columnSpan;

		if (updateRowTemplate) {
			// If we like to update we need to first remove the lock, make the browser render onces and then update.
			(layoutContainer as HTMLElement).style.gridTemplateRows = '';
		}
		//cancelAnimationFrame(raf);
		//raf = requestAnimationFrame(() => {
		// As mentioned above we need to wait until the browser has rendered DOM without the lock of gridTemplateRows.
		this.#updateGridData(layoutContainer, layoutContainerRect, layoutItemRect, updateRowTemplate);
		//});

		// update as we go:
		this.setColumnSpan(newSpans.columnSpan);
		this.setRowSpan(newSpans.rowSpan);
	};

	onScaleMouseUp = (e: MouseEvent) => {
		const layoutContainer = this._entries?.getLayoutContainerElement() as HTMLElement;
		if (!layoutContainer) {
			return;
		}
		//cancelAnimationFrame(raf);

		// Remove listeners:
		window.removeEventListener('mousemove', this.onScaleMouseMove);
		window.removeEventListener('mouseup', this.onScaleMouseUp);
		window.removeEventListener('mouseleave', this.onScaleMouseUp);

		const layoutContainerRect = layoutContainer.getBoundingClientRect();
		const layoutItemRect = this.getHostElement().getBoundingClientRect();

		const startX = layoutItemRect.left - layoutContainerRect.left;
		const startY = layoutItemRect.top - layoutContainerRect.top;
		const endX = e.clientX - layoutContainerRect.left;
		const endY = e.clientY - layoutContainerRect.top;
		const newSpans = this.#getNewSpans(startX, startY, endX, endY);

		// release the lock of gridTemplateRows:
		//layoutContainer.removeChild(scaleBoxBackdropEl);
		//this.scaleBoxBackdropEl = null;
		layoutContainer.style.gridTemplateRows = '';
		//this.#isScaleMode = false;

		// Clean up variables:
		//this.layoutContainer = null;
		//this.gridColumns = [];
		//this.gridRows = [];
		this.#lockedGridRows = 0;

		if (!newSpans) return;
		// Update block size:
		this.setColumnSpan(newSpans.columnSpan);
		this.setRowSpan(newSpans.rowSpan);
	};
}
