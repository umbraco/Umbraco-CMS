/**
 * Visual Editor Injected Script
 *
 * Injected into rendered preview pages to enable visual editing.
 * Discovers annotated property and block regions, adds hover/click/drag interactions,
 * and communicates with the backoffice visual editor via postMessage.
 *
 * This runs in the IFRAME context (the rendered Umbraco page), not in the backoffice.
 *
 * ## Data attributes consumed
 * - `data-umb-block-key` / `data-element-key` — Block content key (identifies a block region)
 * - `data-umb-content-type` / `data-content-element-type-alias` — Block element type alias
 * - `data-umb-property` — Property alias (identifies a property region)
 * - `data-umb-content-key` — Document content key for a property region
 *
 * ## PostMessage protocol (sent to parent)
 * - `umb:ve:region-map` — Discovered regions on page load
 * - `umb:ve:block-selected` — User clicked/edited a block
 * - `umb:ve:block-delete` — User requested block deletion
 * - `umb:ve:block-add` — User clicked an inline create button
 * - `umb:ve:block-reorder` — User drag-sorted a block
 * - `umb:ve:property-selected` — User clicked a property region
 *
 * ## PostMessage protocol (received from parent)
 * - `umb:ve:clear-selection` — Clear all visual selection
 * - `umb:ve:update-property-text` — Optimistic text update for a property
 * - `umb:ve:select-region` — Programmatically select a region
 */
(function () {
	'use strict';

	// =====================================================================
	// Selectors
	// =====================================================================

	/** CSS selector matching block elements annotated by Razor partials. */
	const BLOCK_SELECTOR = '[data-umb-block-key], [data-element-key]';

	/** CSS selector matching property elements annotated by Razor views. */
	const PROP_SELECTOR = '[data-umb-property]';

	/** Combined selector for all editable regions (blocks + properties). */
	const ALL_SELECTOR = `${BLOCK_SELECTOR}, ${PROP_SELECTOR}`;

	/** Data attribute added to injected inline-create buttons. */
	const ADD_BTN_ATTR = 'data-umb-add-block';


	// =====================================================================
	// Colors — mirror UUI design tokens for visual consistency
	// =====================================================================

	/** Primary interactive accent (--uui-color-interactive-emphasis). */
	const COLOR_INTERACTIVE = '#3544b1';

	/** Surface background (--uui-color-surface). */
	const COLOR_SURFACE = '#fff';

	/** Border/divider color (--uui-color-border). */
	const COLOR_BORDER = '#e3e3e3';

	/** Selected region outline color (same as interactive). */
	const COLOR_SELECTED = COLOR_INTERACTIVE;

	// =====================================================================
	// Element helpers — extract data attributes from annotated DOM elements
	// =====================================================================

	/**
	 * Check whether an element is a block region (vs a property region).
	 * @param el - The element to test.
	 * @returns `true` if the element matches the block selector.
	 */
	function isBlock(el: Element): boolean {
		return el.matches(BLOCK_SELECTOR);
	}

	/**
	 * Read the block content key from either `data-umb-block-key` or `data-element-key`.
	 * @param el - A block element.
	 * @returns The block's content key GUID, or empty string if not found.
	 */
	function getBlockKey(el: Element): string {
		return (el as HTMLElement).dataset.umbBlockKey || (el as HTMLElement).dataset.elementKey || '';
	}

	/**
	 * Read the content type alias from `data-umb-content-type` or `data-content-element-type-alias`.
	 * @param el - A block element.
	 * @returns The element type alias, or empty string if not found.
	 */
	function getContentType(el: Element): string {
		return (el as HTMLElement).dataset.umbContentType || (el as HTMLElement).dataset.contentElementTypeAlias || '';
	}

	/**
	 * Read the property alias from `data-umb-property`.
	 * @param el - A property element.
	 * @returns The property alias, or empty string if not found.
	 */
	function getPropertyAlias(el: Element): string {
		return (el as HTMLElement).dataset.umbProperty || '';
	}

	/**
	 * Read the document content key from `data-umb-content-key`.
	 * @param el - A property element.
	 * @returns The content key GUID, or empty string if not found.
	 */
	function getContentKey(el: Element): string {
		return (el as HTMLElement).dataset.umbContentKey || '';
	}

	/**
	 * Build a unique region identifier for an element.
	 * Blocks use `block:{key}`, properties use `prop:{alias}`.
	 * @param el - A block or property element.
	 * @returns A string identifier used for selection tracking.
	 */
	function getRegionId(el: Element): string {
		return isBlock(el) ? `block:${getBlockKey(el)}` : `prop:${getPropertyAlias(el)}`;
	}

	// =====================================================================
	// PostMessage helper
	// =====================================================================

	/**
	 * Send a message to the parent visual editor window.
	 * All messages include `source: 'umb-visual-editor-guest'` so the parent
	 * can filter out unrelated postMessage traffic.
	 * @param message - Key/value payload to send.
	 */
	function send(message: Record<string, unknown>) {
		window.parent.postMessage({ ...message, source: 'umb-visual-editor-guest' }, '*');
	}

	// =====================================================================
	// Region discovery
	// =====================================================================

	/**
	 * Scan the page for all annotated regions (blocks and properties) and
	 * send a `umb:ve:region-map` message to the parent with the results.
	 * Called once on script initialization.
	 */
	function discoverRegions() {
		const regions: Array<Record<string, unknown>> = [];
		document.querySelectorAll(ALL_SELECTOR).forEach((el) => {
			if (isBlock(el)) {
				regions.push({ type: 'block', blockKey: getBlockKey(el), contentTypeAlias: getContentType(el) });
			} else {
				regions.push({ type: 'property', propertyAlias: getPropertyAlias(el), contentKey: getContentKey(el) });
			}
		});
		send({ type: 'umb:ve:region-map', regions });
	}

	// =====================================================================
	// Default block border + cursor
	// =====================================================================

	/**
	 * Apply baseline visual affordances to all editable regions:
	 * - Pointer cursor on all regions
	 * - Subtle dashed border on blocks so they are visually identifiable
	 * - `position: relative` on blocks to anchor the action bar
	 */
	document.querySelectorAll<HTMLElement>(ALL_SELECTOR).forEach((el) => {
		el.style.cursor = 'pointer';
		if (isBlock(el)) {
			el.style.outline = `1px dashed ${COLOR_BORDER}`;
			el.style.outlineOffset = '-1px';
			el.style.position = el.style.position || 'relative';
		}
	});

	// =====================================================================
	// Highlight styles — outline management for hover/selection states
	// =====================================================================

	/** Region ID of the currently selected region, or `null` if nothing is selected. */
	let selectedId: string | null = null;

	/** The element that currently has a hover outline, or `null`. */
	let hoveredRegion: HTMLElement | null = null;

	/**
	 * Apply an outline style to an element.
	 * @param el - The target element.
	 * @param style - CSS outline value (e.g. `'2px solid #3544b1'`).
	 * @param offset - CSS outline-offset value.
	 */
	function applyOutline(el: HTMLElement, style: string, offset: string) {
		el.style.outline = style;
		el.style.outlineOffset = offset;
	}

	/**
	 * Reset an element's outline to its default state.
	 * Blocks return to their subtle dashed border; properties are fully cleared.
	 * @param el - The target element.
	 */
	function clearOutline(el: HTMLElement) {
		if (isBlock(el)) {
			el.style.outline = `1px dashed ${COLOR_BORDER}`;
			el.style.outlineOffset = '-1px';
		} else {
			el.style.outline = '';
			el.style.outlineOffset = '';
		}
	}

	/**
	 * Clear the hover outline on the previously hovered region (if any).
	 * Only clears the single tracked element instead of scanning the entire DOM.
	 */
	function clearHoveredRegion() {
		if (hoveredRegion && getRegionId(hoveredRegion) !== selectedId) {
			clearOutline(hoveredRegion);
		}
		hoveredRegion = null;
	}

	/**
	 * Clear outlines on all regions except the one that is currently selected.
	 * Used when selection state changes and all regions need to be reset.
	 */
	function clearAllExceptSelected() {
		document.querySelectorAll<HTMLElement>(ALL_SELECTOR).forEach((el) => {
			if (getRegionId(el) !== selectedId) clearOutline(el);
		});
		hoveredRegion = null;
	}

	// =====================================================================
	// SVG icons — inline Material Design icons for the action bar buttons
	// =====================================================================

	/** Edit (pencil) icon — 16×16 SVG. */
	const ICON_EDIT =
		'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="16" height="16" fill="currentColor">' +
		'<path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04a1 1 0 000-1.41l-2.34-2.34a1 1 0 00-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"/></svg>';

	/** Delete (trash) icon — 16×16 SVG. */
	const ICON_DELETE =
		'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="16" height="16" fill="currentColor">' +
		'<path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z"/></svg>';

	// =====================================================================
	// Block action bar — pill-shaped edit/delete toolbar inside each block
	//
	// Styled to match `<uui-action-bar>` from @umbraco-ui/uui:
	// - Pill border-radius with rounded ends on first/last buttons
	// - Positioned absolute top-right inside the block (same as
	//   `top: var(--uui-size-2); right: var(--uui-size-2)` in block entries)
	// - Hidden via opacity, fades in on hover (same as
	//   `--umb-block-list-entry-actions-opacity` pattern)
	// =====================================================================

	/** Data attribute used to identify injected action bar elements. */
	const ACTION_BAR_ATTR = 'data-umb-action-bar';

	/**
	 * Create a single action bar button with pill-shaped ends.
	 *
	 * The first button gets a rounded left edge, the last gets a rounded right edge,
	 * mirroring the `::slotted(*:first-child)` / `::slotted(*:last-child)` styling
	 * in `<uui-action-bar>`.
	 * @param title - Tooltip text for the button.
	 * @param icon - Inner HTML (SVG string) for the button icon.
	 * @param isFirst - Whether this is the first button in the bar (rounds left side).
	 * @param isLast - Whether this is the last button in the bar (rounds right side).
	 * @param onClick - Click handler callback.
	 * @returns The constructed button element.
	 */
	function createActionBarBtn(
		title: string,
		icon: string,
		isFirst: boolean,
		isLast: boolean,
		onClick: () => void,
	): HTMLButtonElement {
		const btn = document.createElement('button');
		btn.innerHTML = icon;
		btn.title = title;
		const radiusLeft = isFirst ? '50px' : '0';
		const radiusRight = isLast ? '50px' : '0';
		const paddingLeft = isFirst ? '10px' : '6px';
		const paddingRight = isLast ? '10px' : '6px';
		Object.assign(btn.style, {
			background: 'transparent',
			border: 'none',
			color: '#515054',
			cursor: 'pointer',
			padding: `6px ${paddingRight} 6px ${paddingLeft}`,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			borderRadius: `${radiusLeft} ${radiusRight} ${radiusRight} ${radiusLeft}`,
			transition: 'background 120ms, color 120ms',
			lineHeight: '1',
		});
		btn.addEventListener('mouseenter', () => {
			btn.style.background = '#f3f3f5';
			btn.style.color = COLOR_INTERACTIVE;
		});
		btn.addEventListener('mouseleave', () => {
			btn.style.background = 'transparent';
			btn.style.color = '#515054';
		});
		btn.addEventListener('click', (e) => {
			e.preventDefault();
			e.stopPropagation();
			onClick();
		});
		return btn;
	}

	/**
	 * Create the action bar element for a block.
	 *
	 * Contains an Edit button, a vertical divider, and a Delete button.
	 * The bar is appended as a child of the block element and positioned
	 * absolute in the top-right corner. It starts hidden (`opacity: 0`)
	 * and is shown/hidden by {@link showActionBar} / {@link hideActionBar}.
	 * @param blockEl - The block element to create the action bar for.
	 * @returns The action bar div element.
	 */
	/** Set of content type aliases for blocks with no editable fields (layout-only). */
	const nonEditableTypes = new Set<string>();

	/** Data attribute marking the edit button so it can be toggled later. */
	const EDIT_BTN_ATTR = 'data-umb-edit-btn';
	/** Data attribute marking the divider next to the edit button. */
	const EDIT_DIVIDER_ATTR = 'data-umb-edit-divider';

	function createActionBar(blockEl: Element): HTMLDivElement {
		const bar = document.createElement('div');
		bar.setAttribute(ACTION_BAR_ATTR, '');
		Object.assign(bar.style, {
			position: 'absolute',
			top: '6px',
			right: '6px',
			display: 'flex',
			zIndex: '999999',
			background: COLOR_SURFACE,
			borderRadius: '50px',
			padding: '0',
			gap: '0',
			boxShadow: '0 1px 3px rgba(0,0,0,0.15), 0 1px 2px rgba(0,0,0,0.1)',
			alignItems: 'center',
			pointerEvents: 'auto',
			border: `1px solid ${COLOR_BORDER}`,
			opacity: '0',
			transition: 'opacity 120ms',
		});

		const blockKey = getBlockKey(blockEl);
		const contentTypeAlias = getContentType(blockEl);
		const isNonEditable = contentTypeAlias ? nonEditableTypes.has(contentTypeAlias) : false;

		const editBtn = createActionBarBtn('Edit', ICON_EDIT, true, false, () => {
			send({ type: 'umb:ve:block-selected', blockKey, contentTypeAlias });
		});
		editBtn.setAttribute(EDIT_BTN_ATTR, '');
		if (isNonEditable) editBtn.style.display = 'none';
		bar.appendChild(editBtn);

		// Vertical divider between buttons
		const divider = document.createElement('span');
		divider.setAttribute(EDIT_DIVIDER_ATTR, '');
		Object.assign(divider.style, {
			width: '1px',
			height: '16px',
			background: COLOR_BORDER,
			flexShrink: '0',
		});
		if (isNonEditable) divider.style.display = 'none';
		bar.appendChild(divider);

		bar.appendChild(
			createActionBarBtn('Delete', ICON_DELETE, false, true, () => {
				send({ type: 'umb:ve:block-delete', blockKey });
			}),
		);

		return bar;
	}

	// Attach action bars to all blocks upfront (hidden via opacity).
	document.querySelectorAll<HTMLElement>(BLOCK_SELECTOR).forEach((block) => {
		block.appendChild(createActionBar(block));
	});

	/** The block element whose action bar is currently visible, or `null`. */
	let actionBarTarget: Element | null = null;

	/**
	 * Show the action bar for a block by setting its opacity to 1.
	 * Hides any previously visible action bar first.
	 * @param blockEl - The block element to show the action bar for.
	 */
	function showActionBar(blockEl: Element) {
		if (actionBarTarget === blockEl) return;
		hideActionBar();
		actionBarTarget = blockEl;
		const bar = blockEl.querySelector<HTMLElement>(`[${ACTION_BAR_ATTR}]`);
		if (bar) bar.style.opacity = '1';
	}

	/**
	 * Hide the currently visible action bar by setting its opacity to 0.
	 */
	function hideActionBar() {
		if (actionBarTarget) {
			const bar = actionBarTarget.querySelector<HTMLElement>(`[${ACTION_BAR_ATTR}]`);
			if (bar) bar.style.opacity = '0';
		}
		actionBarTarget = null;
	}

	// =====================================================================
	// Hover — highlight regions and show/hide action bars on mouseover
	// =====================================================================

	/**
	 * Check whether an element is part of an action bar.
	 * Used to prevent hover/click handlers from interfering with action bar interactions.
	 * @param el - The element to test (may be null).
	 * @returns `true` if the element is inside an action bar.
	 */
	function isActionBar(el: Element | null): boolean {
		return !!el?.closest?.(`[${ACTION_BAR_ATTR}]`);
	}

	/** Mouseover handler — applies hover outlines and shows block action bars. */
	document.addEventListener('mouseover', (e: MouseEvent) => {
		const target = e.target as HTMLElement | null;
		if (!target) return;

		// Don't interfere when hovering over action bar buttons
		if (isActionBar(target)) return;

		const region = target.closest<HTMLElement>(ALL_SELECTOR);

		clearHoveredRegion();
		if (region && getRegionId(region) !== selectedId) {
			hoveredRegion = region;
			if (isBlock(region)) {
				applyOutline(region, `2px solid ${COLOR_INTERACTIVE}`, '-2px');
			} else {
				applyOutline(region, '2px dashed rgba(59, 130, 246, 0.4)', '-2px');
			}
		}

		if (region && isBlock(region)) {
			showActionBar(region);
		} else if (!isActionBar(e.relatedTarget as Element | null)) {
			hideActionBar();
		}
	});

	/** Mouseout handler — clears hover outlines and hides action bars when leaving blocks. */
	document.addEventListener('mouseout', (e: MouseEvent) => {
		const target = e.target as HTMLElement | null;
		if (!target) return;
		const region = target.closest<HTMLElement>(ALL_SELECTOR);
		if (region && getRegionId(region) !== selectedId) clearOutline(region);

		const related = e.relatedTarget as HTMLElement | null;
		if (!isActionBar(related) && !related?.closest?.(BLOCK_SELECTOR)) {
			hideActionBar();
		}
	});

	// =====================================================================
	// Click — select regions and notify the parent visual editor
	// =====================================================================

	/**
	 * Click handler (capture phase) — selects a region and sends the
	 * appropriate message to the parent visual editor.
	 *
	 * Blocks send `umb:ve:block-selected`, properties send `umb:ve:property-selected`.
	 * Clicks on action bar buttons are ignored (they have their own handlers).
	 */
	document.addEventListener(
		'click',
		(e: MouseEvent) => {
			const target = e.target as HTMLElement | null;
			if (!target) return;
			if (isActionBar(target)) return;
			if (target.closest(`[${ADD_BTN_ATTR}]`)) return;

			const region = target.closest<HTMLElement>(ALL_SELECTOR);
			if (!region) return;
			e.preventDefault();
			e.stopPropagation();

			document.querySelectorAll<HTMLElement>(ALL_SELECTOR).forEach((el) => clearOutline(el));

			selectedId = getRegionId(region);
			applyOutline(region, `2px solid ${COLOR_SELECTED}`, '-2px');

			if (isBlock(region)) {
				send({
					type: 'umb:ve:block-selected',
					blockKey: getBlockKey(region),
					contentTypeAlias: getContentType(region),
				});
			} else {
				send({
					type: 'umb:ve:property-selected',
					propertyAlias: getPropertyAlias(region),
					contentKey: getContentKey(region),
				});
			}
		},
		true,
	);

	// =====================================================================
	// Messages from parent — handle commands from the visual editor host
	// =====================================================================

	/**
	 * Message handler for commands sent from the parent visual editor window.
	 *
	 * Supported message types:
	 * - `umb:ve:clear-selection` — Deselect all regions
	 * - `umb:ve:update-property-text` — Optimistically update a property's text content
	 * - `umb:ve:select-region` — Programmatically select and scroll to a region
	 */
	window.addEventListener('message', (evt: MessageEvent) => {
		if (!evt.data) return;

		if (evt.data.type === 'umb:ve:clear-selection') {
			selectedId = null;
			document.querySelectorAll<HTMLElement>(ALL_SELECTOR).forEach((el) => clearOutline(el));
		}

		if (evt.data.type === 'umb:ve:update-property-text' && evt.data.propertyAlias) {
			document.querySelectorAll<HTMLElement>(`[data-umb-property="${evt.data.propertyAlias}"]`).forEach((span) => {
				span.textContent = evt.data.value;
			});
		}

		if (evt.data.type === 'umb:ve:non-editable-types' && Array.isArray(evt.data.aliases)) {
			nonEditableTypes.clear();
			for (const alias of evt.data.aliases) nonEditableTypes.add(alias);

			// Update existing action bars to hide/show edit buttons
			document.querySelectorAll<HTMLElement>(BLOCK_SELECTOR).forEach((blockEl) => {
				const alias = getContentType(blockEl);
				const hidden = alias ? nonEditableTypes.has(alias) : false;
				const bar = blockEl.querySelector<HTMLElement>(`[${ACTION_BAR_ATTR}]`);
				if (!bar) return;
				const editBtn = bar.querySelector<HTMLElement>(`[${EDIT_BTN_ATTR}]`);
				const divider = bar.querySelector<HTMLElement>(`[${EDIT_DIVIDER_ATTR}]`);
				if (editBtn) editBtn.style.display = hidden ? 'none' : '';
				if (divider) divider.style.display = hidden ? 'none' : '';
			});
		}

		if (evt.data.type === 'umb:ve:select-region' && evt.data.regionId) {
			selectedId = evt.data.regionId;
			document.querySelectorAll<HTMLElement>(ALL_SELECTOR).forEach((el) => {
				if (getRegionId(el) === selectedId) {
					applyOutline(el, `2px solid ${COLOR_SELECTED}`, '-2px');
					el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
				} else {
					clearOutline(el);
				}
			});
		}
	});

	// =====================================================================
	// Block drag-to-sort — native HTML5 drag and drop for reordering blocks
	// =====================================================================

	/**
	 * Sets up drag-to-sort on all block elements, supporting both
	 * same-container reordering and cross-container moves (e.g. dragging
	 * a block from root into an area, or between areas).
	 */
	(function () {
		const blocks = document.querySelectorAll<HTMLElement>(BLOCK_SELECTOR);
		let dragSrc: HTMLElement | null = null;

		const dropIndicator = document.createElement('div');
		Object.assign(dropIndicator.style, {
			height: '3px',
			background: COLOR_INTERACTIVE,
			borderRadius: '2px',
			margin: '2px 0',
			pointerEvents: 'none',
			display: 'none',
		});

		/** Get the immediate block container for an element. */
		function getContainer(el: Element): Element | null {
			return el.closest('.umb-block-list, .umb-block-grid__layout-container');
		}

		/** Get area info for a container (if it's inside an area). */
		function getAreaInfo(container: Element): { parentBlockKey: string; areaAlias: string } | null {
			const area = container.closest<HTMLElement>('.umb-block-grid__area');
			if (!area) return null;
			const parentBlock = area.closest<HTMLElement>(BLOCK_SELECTOR);
			if (!parentBlock) return null;
			return {
				parentBlockKey: getBlockKey(parentBlock),
				areaAlias: area.dataset.areaAlias || '',
			};
		}

		blocks.forEach((block) => {
			block.setAttribute('draggable', 'true');

			block.addEventListener('dragstart', ((e: DragEvent) => {
				dragSrc = block;
				block.style.opacity = '0.4';
				e.dataTransfer!.effectAllowed = 'move';
				e.dataTransfer!.setData('text/plain', getBlockKey(block));
			}) as EventListener);

			block.addEventListener('dragend', () => {
				if (dragSrc) dragSrc.style.opacity = '';
				dragSrc = null;
				dropIndicator.style.display = 'none';
				dropIndicator.parentNode?.removeChild(dropIndicator);
			});

			block.addEventListener('dragover', ((e: DragEvent) => {
				if (!dragSrc || dragSrc === block) return;
				e.preventDefault();
				e.dataTransfer!.dropEffect = 'move';

				const rect = block.getBoundingClientRect();
				const midY = rect.top + rect.height / 2;
				const parent = block.parentNode!;

				if (e.clientY < midY) {
					parent.insertBefore(dropIndicator, block);
				} else {
					parent.insertBefore(dropIndicator, block.nextSibling);
				}
				dropIndicator.style.display = 'block';
			}) as EventListener);

			block.addEventListener('drop', ((e: DragEvent) => {
				e.preventDefault();
				if (!dragSrc || dragSrc === block) return;

				const srcKey = getBlockKey(dragSrc);
				const srcContainer = getContainer(dragSrc);
				const targetContainer = getContainer(block);
				const sameContainer = srcContainer === targetContainer;

				const parent = block.parentNode!;
				const allBlocks = Array.from(parent.querySelectorAll<HTMLElement>(`:scope > ${BLOCK_SELECTOR}`));

				const rect = block.getBoundingClientRect();
				const midY = rect.top + rect.height / 2;
				let targetIndex = allBlocks.indexOf(block);
				if (e.clientY >= midY) targetIndex++;

				if (sameContainer) {
					// Same-container reorder
					const fromIndex = allBlocks.indexOf(dragSrc);
					if (fromIndex < targetIndex) targetIndex--;

					if (dropIndicator.parentNode) {
						parent.insertBefore(dragSrc, dropIndicator);
						dropIndicator.style.display = 'none';
						parent.removeChild(dropIndicator);
					}

					dragSrc.style.opacity = '';
					dragSrc = null;
					insertAddButtons();

					send({ type: 'umb:ve:block-reorder', blockKey: srcKey, fromIndex, toIndex: targetIndex });
				} else {
					// Cross-container move
					if (dropIndicator.parentNode) {
						parent.insertBefore(dragSrc, dropIndicator);
						dropIndicator.style.display = 'none';
						parent.removeChild(dropIndicator);
					}

					dragSrc.style.opacity = '';
					dragSrc = null;
					insertAddButtons();

					const targetAreaInfo = targetContainer ? getAreaInfo(targetContainer) : null;
					send({
						type: 'umb:ve:block-move',
						blockKey: srcKey,
						targetIndex,
						targetParentBlockKey: targetAreaInfo?.parentBlockKey,
						targetAreaAlias: targetAreaInfo?.areaAlias,
					});
				}
			}) as EventListener);
		});

		// Make empty areas accept drops
		document.querySelectorAll<HTMLElement>('.umb-block-grid__area').forEach((area) => {
			area.addEventListener('dragover', ((e: DragEvent) => {
				if (!dragSrc) return;
				// Only handle if the area itself is the target (not a block inside it)
				if ((e.target as Element).closest(BLOCK_SELECTOR)) return;
				e.preventDefault();
				e.dataTransfer!.dropEffect = 'move';

				if (!area.contains(dropIndicator)) {
					dropIndicator.style.display = 'block';
					area.appendChild(dropIndicator);
				}
			}) as EventListener);

			area.addEventListener('drop', ((e: DragEvent) => {
				if (!dragSrc) return;
				// Only handle direct drops on the area (not on blocks inside it)
				if ((e.target as Element).closest(BLOCK_SELECTOR)) return;
				e.preventDefault();

				const srcKey = getBlockKey(dragSrc);
				const parentBlock = area.closest<HTMLElement>(BLOCK_SELECTOR);
				const parentBlockKey = parentBlock ? getBlockKey(parentBlock) : '';
				const areaAlias = area.dataset.areaAlias || '';

				// Move DOM
				area.insertBefore(dragSrc, dropIndicator);
				dropIndicator.style.display = 'none';
				dropIndicator.parentNode?.removeChild(dropIndicator);

				dragSrc.style.opacity = '';
				dragSrc = null;
				insertAddButtons();

				send({
					type: 'umb:ve:block-move',
					blockKey: srcKey,
					targetIndex: 0,
					targetParentBlockKey: parentBlockKey || undefined,
					targetAreaAlias: areaAlias || undefined,
				});
			}) as EventListener);
		});
	})();

	// =====================================================================
	// Inline create buttons — styled to match `<uui-button-inline-create>`
	//
	// Each button is a 20px-tall invisible hit area with negative margins
	// so it overlaps between adjacent blocks. On hover, a horizontal line
	// and a circular "+" icon animate in using the same elastic easing and
	// scale transitions as the UUI component.
	// =====================================================================

	/**
	 * Create an inline "add block" button to insert between blocks.
	 *
	 * The button consists of two child elements:
	 * - A horizontal line (`<span>`) spanning the full width
	 * - A circular plus icon (`<span>`) centered on the line
	 *
	 * Both are hidden by default and animate in on hover.
	 * @param index - The insertion index for the new block.
	 * @param siblingKey - Content key of a sibling block (used to identify the parent property).
	 * @returns The constructed button element.
	 */
	function createAddButton(index: number, siblingKey: string): HTMLButtonElement {
		const btn = document.createElement('button');
		btn.setAttribute(ADD_BTN_ATTR, '');
		btn.title = 'Add block';
		Object.assign(btn.style, {
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			width: '100%',
			height: '20px',
			margin: '-10px 0',
			padding: '0',
			border: 'none',
			background: 'transparent',
			cursor: 'pointer',
			position: 'relative',
			zIndex: '1',
		});

		// Horizontal line spanning the full width
		const line = document.createElement('span');
		Object.assign(line.style, {
			position: 'absolute',
			left: '0',
			right: '0',
			height: '2px',
			background: COLOR_INTERACTIVE,
			opacity: '0',
			transition: 'opacity 200ms',
		});
		btn.appendChild(line);

		// Circular plus icon centered on the line
		const plus = document.createElement('span');
		Object.assign(plus.style, {
			position: 'relative',
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			width: '28px',
			height: '28px',
			borderRadius: '50%',
			border: `2px solid ${COLOR_INTERACTIVE}`,
			background: COLOR_SURFACE,
			color: COLOR_INTERACTIVE,
			fontSize: '18px',
			fontWeight: '300',
			lineHeight: '1',
			opacity: '0',
			transform: 'scale(0)',
			transition:
				'transform 240ms cubic-bezier(0.175, 0.885, 0.32, 1.275), opacity 80ms, box-shadow 240ms cubic-bezier(0.175, 0.885, 0.32, 1.275)',
			boxShadow: `0 0 0 0px ${COLOR_SURFACE}`,
		});
		plus.textContent = '+';
		btn.appendChild(plus);

		// Hover: show line + scale in the plus icon with surface halo
		btn.addEventListener('mouseenter', () => {
			line.style.opacity = '1';
			plus.style.opacity = '1';
			plus.style.transform = 'scale(1)';
			plus.style.boxShadow = `0 0 0 2px ${COLOR_SURFACE}`;
		});
		btn.addEventListener('mouseleave', () => {
			line.style.opacity = '0';
			plus.style.opacity = '0';
			plus.style.transform = 'scale(0)';
			plus.style.boxShadow = `0 0 0 0px ${COLOR_SURFACE}`;
		});

		// Press feedback: slight scale bump
		btn.addEventListener('mousedown', () => {
			plus.style.transform = 'scale(1.1)';
		});
		btn.addEventListener('mouseup', () => {
			plus.style.transform = 'scale(1)';
		});

		btn.addEventListener('click', (e) => {
			e.preventDefault();
			e.stopPropagation();
			send({ type: 'umb:ve:block-add', siblingBlockKey: siblingKey, insertIndex: index });
		});
		return btn;
	}

	/**
	 * Create an "add block" button for an empty block grid area.
	 * Reuses the same visual style as `createAddButton` but sends
	 * `umb:ve:block-add-to-area` with area-specific data.
	 */
	function createAreaAddButton(parentBlockKey: string, areaAlias: string): HTMLButtonElement {
		const btn = createAddButton(0, '');
		// Replace the click listener with area-specific one.
		// Clone to remove existing listeners, then re-add hover + click.
		const fresh = btn.cloneNode(true) as HTMLButtonElement;
		fresh.setAttribute(ADD_BTN_ATTR, '');

		const line = fresh.children[0] as HTMLElement;
		const plus = fresh.children[1] as HTMLElement;

		fresh.addEventListener('mouseenter', () => {
			if (line) line.style.opacity = '1';
			if (plus) {
				plus.style.opacity = '1';
				plus.style.transform = 'scale(1)';
				plus.style.boxShadow = `0 0 0 2px ${COLOR_SURFACE}`;
			}
		});
		fresh.addEventListener('mouseleave', () => {
			if (line) line.style.opacity = '0';
			if (plus) {
				plus.style.opacity = '0';
				plus.style.transform = 'scale(0)';
				plus.style.boxShadow = `0 0 0 0px ${COLOR_SURFACE}`;
			}
		});
		fresh.addEventListener('click', (e) => {
			e.preventDefault();
			e.stopPropagation();
			send({ type: 'umb:ve:block-add-to-area', parentBlockKey, areaAlias, insertIndex: 0 });
		});
		return fresh;
	}

	/**
	 * Insert inline create buttons between all blocks in block list and
	 * block grid containers.
	 *
	 * Removes any previously injected buttons first, then scans for
	 * `.umb-block-list` and `.umb-block-grid__layout-container` elements
	 * and inserts a button before each block and after the last block.
	 *
	 * Also called after drag-to-sort to re-insert buttons at the correct positions.
	 */
	/**
	 * Create a prominent "Add content" placeholder button for empty containers.
	 * Styled as a dashed border box with a centered label, similar to the
	 * backoffice's empty state pattern.
	 *
	 * @param onClick - Click handler for the button.
	 * @param label - Button label text.
	 */
	function createEmptyPlaceholder(onClick: () => void, label = 'Add content'): HTMLButtonElement {
		const btn = document.createElement('button');
		btn.setAttribute(ADD_BTN_ATTR, '');
		btn.textContent = label;
		Object.assign(btn.style, {
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			gap: '8px',
			width: '100%',
			minHeight: '80px',
			padding: '16px',
			border: `2px dashed ${COLOR_BORDER}`,
			borderRadius: '6px',
			background: 'transparent',
			color: COLOR_INTERACTIVE,
			fontSize: '14px',
			fontWeight: '500',
			cursor: 'pointer',
			transition: 'border-color 150ms, background 150ms',
		});
		btn.addEventListener('mouseenter', () => {
			btn.style.borderColor = COLOR_INTERACTIVE;
			btn.style.background = `color-mix(in srgb, ${COLOR_INTERACTIVE} 5%, transparent)`;
		});
		btn.addEventListener('mouseleave', () => {
			btn.style.borderColor = COLOR_BORDER;
			btn.style.background = 'transparent';
		});
		btn.addEventListener('click', (e) => {
			e.preventDefault();
			e.stopPropagation();
			onClick();
		});
		return btn;
	}

	function insertAddButtons() {
		document.querySelectorAll(`[${ADD_BTN_ATTR}]`).forEach((btn) => btn.remove());

		document.querySelectorAll('.umb-block-list, .umb-block-grid__layout-container').forEach((container) => {
			const blocks = container.querySelectorAll<HTMLElement>(`:scope > ${BLOCK_SELECTOR}`);

			if (blocks.length === 0) {
				// Empty container — show a prominent "Add content" placeholder.
				// Find a sibling block key from the closest block ancestor (for areas)
				// or use empty string (for root-level empty lists).
				const parentBlock = container.closest<HTMLElement>(BLOCK_SELECTOR);
				const areaEl = container.closest<HTMLElement>('.umb-block-grid__area');

				if (areaEl && parentBlock) {
					// Empty area inside a block grid
					const parentBlockKey = getBlockKey(parentBlock);
					const areaAlias = areaEl.dataset.areaAlias || '';
					if (parentBlockKey && areaAlias) {
						container.appendChild(
							createEmptyPlaceholder(() => {
								send({ type: 'umb:ve:block-add-to-area', parentBlockKey, areaAlias, insertIndex: 0 });
							}),
						);
					}
				}
				return;
			}

			const siblingBlockKey = blocks[0].dataset.umbBlockKey || blocks[0].dataset.elementKey || '';

			blocks.forEach((block, i) => {
				block.parentNode!.insertBefore(createAddButton(i, siblingBlockKey), block);
			});
			blocks[blocks.length - 1].parentNode!.appendChild(createAddButton(blocks.length, siblingBlockKey));
		});

		// Also handle empty block grid areas that have NO layout-container child at all
		document.querySelectorAll<HTMLElement>('.umb-block-grid__area').forEach((area) => {
			const hasContainer = area.querySelector('.umb-block-grid__layout-container');
			if (hasContainer) return;

			const areaAlias = area.dataset.areaAlias || '';
			const parentBlock = area.closest<HTMLElement>(BLOCK_SELECTOR);
			const parentBlockKey = parentBlock ? getBlockKey(parentBlock) : '';
			if (!parentBlockKey || !areaAlias) return;

			area.appendChild(
				createEmptyPlaceholder(() => {
					send({ type: 'umb:ve:block-add-to-area', parentBlockKey, areaAlias, insertIndex: 0 });
				}),
			);
		});

		// Empty block lists at root level (not inside a grid area)
		document.querySelectorAll<HTMLElement>('.umb-block-list').forEach((list) => {
			if (list.querySelector(BLOCK_SELECTOR)) return; // Has blocks
			if (list.querySelector(`[${ADD_BTN_ATTR}]`)) return; // Already handled

			// Find a sibling block key — for root-level empty lists there's none,
			// so we can't determine the property alias. This is a limitation.
			// TODO: annotate block list containers with property alias for empty state support.
		});
	}

	// =====================================================================
	// Initialization
	// =====================================================================

	insertAddButtons();
	discoverRegions();
})();
