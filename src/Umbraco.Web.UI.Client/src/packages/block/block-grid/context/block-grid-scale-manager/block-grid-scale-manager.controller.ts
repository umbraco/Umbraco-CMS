import { closestColumnSpanOption } from '../../utils/index.js';
import { getAccumulatedValueOfIndex, getInterpolatedIndexOfPositionInWeightMap } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// This might be more generic than Block Grid, but this is where it belongs currently:
export interface UmbBlockGridScalableContext extends UmbControllerHost {
	setColumnSpan: (columnSpan: number) => void;
	setRowSpan: (rowSpan: number) => void;
	getColumnSpan: () => number | undefined;
	getRowSpan: () => number | undefined;
	getMinMaxRowSpan: () => [number, number] | undefined;
	getRelevantColumnSpanOptions: () => Array<number> | undefined;
}

// This might be more generic than Block Grid, but this is where it belongs currently:
export interface UmbBlockGridScalableContainerContext extends UmbControllerHost {
	getLayoutColumns: () => number | undefined;
	getLayoutContainerElement: () => HTMLElement | undefined;
}

export class UmbBlockGridScaleManager extends UmbControllerBase {
	//
	#runtimeGridColumns: Array<number> = [];
	#runtimeGridRows: Array<number> = [];
	#lockedGridRows = 0;
	//
	override _host: UmbBlockGridScalableContext;
	_entries?: UmbBlockGridScalableContainerContext;

	constructor(host: UmbBlockGridScalableContext) {
		super(host, 'blockGridScaleManager');
		this._host = host;
	}

	setEntriesContext(entriesContext: UmbBlockGridScalableContainerContext | undefined) {
		this._entries = entriesContext;
	}

	// Scaling feature:

	#updateGridData(
		layoutContainer: HTMLElement,
		layoutContainerRect: DOMRect,
		layoutItemRect: DOMRect,
		updateRowTemplate: boolean,
	) {
		if (!this._entries) return;
		const layoutColumns = this._entries.getLayoutColumns() ?? 0;

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
			while (i++ < (this._host.getRowSpan() ?? 0)) {
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

		const columnOptions = this._host.getRelevantColumnSpanOptions();
		if (!columnOptions) return;

		// Find nearest allowed Column:
		const bestColumnSpanOption = closestColumnSpanOption(newColumnSpan, columnOptions, layoutColumns - blockStartCol);
		newColumnSpan = bestColumnSpanOption ?? layoutColumns;

		// Find allowed row spans:
		const minMaxRowSpan = this._host.getMinMaxRowSpan();
		if (!minMaxRowSpan) return;
		const [rowMinSpan, rowMaxSpan] = minMaxRowSpan;
		let newRowSpan = Math.round(Math.max(blockEndRow - blockStartRow, rowMinSpan));
		if (rowMaxSpan != null) {
			newRowSpan = Math.min(newRowSpan, rowMaxSpan);
		}

		return { columnSpan: newColumnSpan, rowSpan: newRowSpan, startCol: blockStartCol, startRow: blockStartRow };
	}

	public onScaleMouseDown(event: MouseEvent) {
		const layoutContainer = this._entries?.getLayoutContainerElement();
		if (!layoutContainer) {
			return;
		}
		event.preventDefault();

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

	#updateLayoutRaf = 0;
	onScaleMouseMove = (e: MouseEvent) => {
		const layoutContainer = this._entries?.getLayoutContainerElement();
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

		const updateRowTemplate = this._host.getColumnSpan() !== newSpans.columnSpan;
		if (updateRowTemplate) {
			// If we like to update we need to first remove the lock, make the browser render onces and then update.
			(layoutContainer as HTMLElement).style.gridTemplateRows = '';
		}
		cancelAnimationFrame(this.#updateLayoutRaf);
		this.#updateLayoutRaf = requestAnimationFrame(() => {
			// As mentioned above we need to wait until the browser has rendered DOM without the lock of gridTemplateRows.
			this.#updateGridData(layoutContainer, layoutContainerRect, layoutItemRect, updateRowTemplate);
		});

		// update as we go:
		this._host.setColumnSpan(newSpans.columnSpan);
		this._host.setRowSpan(newSpans.rowSpan);
	};

	onScaleMouseUp = (e: MouseEvent) => {
		const layoutContainer = this._entries?.getLayoutContainerElement();
		if (!layoutContainer) {
			return;
		}
		cancelAnimationFrame(this.#updateLayoutRaf);

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
		this._host.setColumnSpan(newSpans.columnSpan);
		this._host.setRowSpan(newSpans.rowSpan);
	};
}

export default UmbBlockGridScaleManager;
