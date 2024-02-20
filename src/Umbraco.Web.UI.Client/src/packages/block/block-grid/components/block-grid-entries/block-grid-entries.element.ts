import { UmbBlockGridEntriesContext } from '../../context/block-grid-entries.context.js';
import type { UmbBlockGridEntryElement } from '../block-grid-entry/index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbBlockGridLayoutModel } from '@umbraco-cms/backoffice/block';
import { html, customElement, state, repeat, css, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '../block-grid-entry/index.js';
import {
	UmbSorterController,
	type UmbSorterConfig,
	type resolveVerticalDirectionArgs,
} from '@umbraco-cms/backoffice/sorter';

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

function resolveVerticalDirectionAsGrid(
	args: resolveVerticalDirectionArgs<UmbBlockGridLayoutModel, UmbBlockGridEntryElement>,
) {
	/** We need some data about the grid to figure out if there is room to be placed next to the found element */
	const approvedContainerComputedStyles = getComputedStyle(args.containerElement);
	const gridColumnGap = Number(approvedContainerComputedStyles.columnGap.split('px')[0]) || 0;
	const gridColumnNumber = parseInt(
		approvedContainerComputedStyles.getPropertyValue('--umb-block-grid--grid-columns'),
		10,
	);

	const foundElColumns = parseInt(args.relatedElement.dataset.colSpan ?? '', 10);
	const currentElementColumns = args.item.columnSpan;

	if (currentElementColumns >= gridColumnNumber) {
		return true;
	}

	// Get grid template:
	const approvedContainerGridColumns = approvedContainerComputedStyles.gridTemplateColumns
		.trim()
		.split('px')
		.map((x) => Number(x))
		.filter((n) => n > 0)
		.map((n, i, list) => (list.length === i ? n : n + gridColumnGap));

	// ensure all columns are there.
	// This will also ensure handling non-css-grid mode,
	// use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
	let amountOfColumnsInWeightMap = approvedContainerGridColumns.length;
	const amountOfUnknownColumns = gridColumnNumber - amountOfColumnsInWeightMap;
	if (amountOfUnknownColumns > 0) {
		const accumulatedValue = getAccumulatedValueOfIndex(amountOfColumnsInWeightMap, approvedContainerGridColumns) || 0;
		const missingColumnWidth = (args.containerRect.width - accumulatedValue) / amountOfUnknownColumns;
		if (missingColumnWidth > 0) {
			while (amountOfColumnsInWeightMap++ < gridColumnNumber) {
				approvedContainerGridColumns.push(missingColumnWidth);
			}
		}
	}

	let offsetPlacement = 0;
	/* If placeholder is in this same line, we want to assume that it will offset the placement of the found element,
	which provides more potential space for the item to drop at.
	This is relevant in this calculation where we look at the space to determine if its a vertical or horizontal drop in relation to the found element.
	*/
	if (args.placeholderIsInThisRow && args.elementRect.left < args.relatedRect.left) {
		offsetPlacement = -(args.elementRect.width + gridColumnGap);
	}

	const relatedStartX = Math.max(args.relatedRect.left - args.containerRect.left + offsetPlacement, 0);
	const relatedStartCol = Math.round(
		getInterpolatedIndexOfPositionInWeightMap(relatedStartX, approvedContainerGridColumns),
	);

	// If the found related element does not have enough room after which for the current element, then we go vertical mode:
	return relatedStartCol + (args.horizontalPlaceAfter ? foundElColumns : 0) + currentElementColumns > gridColumnNumber;
}

// --------------------------
// End of utils.
// --------------------------

const SORTER_CONFIG: UmbSorterConfig<UmbBlockGridLayoutModel, UmbBlockGridEntryElement> = {
	getUniqueOfElement: (element) => {
		return element.contentUdi!;
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.contentUdi;
	},
	resolveVerticalDirection: resolveVerticalDirectionAsGrid,
	identifier: 'block-grid-editor',
	itemSelector: 'umb-block-grid-entry',
	containerSelector: '.umb-block-grid__layout-container',
};

/**
 * @element umb-block-grid-entries
 */
@customElement('umb-block-grid-entries')
export class UmbBlockGridEntriesElement extends UmbLitElement {
	//
	// TODO: Make sure Sorter callbacks handles columnSpan when retrieving a new entry.

	//
	#sorter = new UmbSorterController<UmbBlockGridLayoutModel, UmbBlockGridEntryElement>(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			this.#context.setLayouts(model);
		},
		onRequestMove: ({ item }) => {
			// TODO: implement
			return true;
		},
		onDisallowed: () => {
			this.setAttribute('disallow-drop', '');
		},
		onAllowed: () => {
			this.removeAttribute('disallow-drop');
		},
	});

	#context = new UmbBlockGridEntriesContext(this);

	@property({ attribute: false })
	public set areaKey(value: string | null | undefined) {
		this._areaKey = value;
		this.#context.setAreaKey(value ?? null);
	}
	public get areaKey(): string | null | undefined {
		return this._areaKey;
	}

	@property({ attribute: false })
	public set layoutColumns(value: number | undefined) {
		this.#context.setLayoutColumns(value);
	}
	public get layoutColumns(): number | undefined {
		return this.#context.getLayoutColumns();
	}

	@state()
	private _areaKey?: string | null;

	@state()
	private _styleElement?: HTMLLinkElement;

	@state()
	private _layoutEntries: Array<UmbBlockGridLayoutModel> = [];

	constructor() {
		super();
		this.observe(this.#context.layoutEntries, (layoutEntries) => {
			const oldValue = this._layoutEntries;
			this.#sorter.setModel(layoutEntries);
			this._layoutEntries = layoutEntries;
			this.requestUpdate('layoutEntries', oldValue);
		});

		this.#context.getManager().then((manager) => {
			this.observe(
				manager.layoutStylesheet,
				(stylesheet) => {
					if (this._styleElement && this._styleElement.href === stylesheet) return;
					this._styleElement = document.createElement('link');
					this._styleElement.setAttribute('rel', 'stylesheet');
					this._styleElement.setAttribute('href', stylesheet);
				},
				'observeStylesheet',
			);
		});
	}

	// TODO: Missing ability to jump directly to creating a Block, when there is only one Block Type.
	render() {
		return html`
			${this._styleElement}
			<div class="umb-block-grid__layout-container">
				${repeat(
					this._layoutEntries,
					(x) => x.contentUdi,
					(layoutEntry, index) =>
						html`<umb-block-grid-entry
							class="umb-block-grid__layout-item"
							.index=${index}
							.contentUdi=${layoutEntry.contentUdi}
							.layout=${layoutEntry}>
						</umb-block-grid-entry>`,
				)}
			</div>
			${this._areaKey === null || this._layoutEntries.length === 0
				? html` <uui-button-group>
						<uui-button
							id="add-button"
							look="placeholder"
							label=${this.localize.term('blockEditor_addBlock')}
							href=${this.#context.getPathForCreateBlock(-1) ?? ''}></uui-button>
						${this._areaKey === null
							? html` <uui-button
									label=${this.localize.term('content_createFromClipboard')}
									look="placeholder"
									href=${this.#context.getPathForClipboard(-1) ?? ''}>
									<uui-icon name="icon-paste-in"></uui-icon>
							  </uui-button>`
							: ''}
				  </uui-button-group>`
				: html`
						<uui-button-inline-create
							href=${this.#context.getPathForCreateBlock(-1) ?? ''}
							label=${this.localize.term('blockEditor_addBlock')}></uui-button-inline-create>
				  `}
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				position: relative;
				display: grid;
				gap: 1px;
			}
			:host([disallow-drop])::before {
				content: '';
				position: absolute;
				z-index: 1;
				inset: 0;
				border: 2px solid var(--uui-color-danger);
				border-radius: calc(var(--uui-border-radius) * 2);
				pointer-events: none;
			}
			:host([disallow-drop])::after {
				content: '';
				position: absolute;
				z-index: 1;
				inset: 0;
				border-radius: calc(var(--uui-border-radius) * 2);
				background-color: var(--uui-color-danger);
				opacity: 0.2;
				pointer-events: none;
			}
			> div {
				display: flex;
				flex-direction: column;
				align-items: stretch;
			}

			uui-button-group {
				padding-top: 1px;
				display: grid;
				grid-template-columns: 1fr auto;
			}
		`,
	];
}

export default UmbBlockGridEntriesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-entries': UmbBlockGridEntriesElement;
	}
}
