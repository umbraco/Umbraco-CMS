import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '@umbraco-cms/backoffice/block-grid';

/**
 * Helper for reading and manipulating block value JSON in the workspace context.
 * Block List and Block Grid store their data as a JSON structure with
 * layout, contentData, settingsData, and expose arrays.
 */

export interface BlockValueLayout {
	contentKey: string;
	settingsKey?: string | null;
	// Block Grid adds: rowSpan, columnSpan, areas, etc.
	[key: string]: unknown;
}

export interface BlockValueData {
	key: string;
	contentTypeKey: string;
	values: Array<{ alias: string; value: unknown; editorAlias?: string; culture?: string | null; segment?: string | null }>;
}

export interface BlockValue {
	layout: Record<string, BlockValueLayout[] | undefined>;
	contentData: BlockValueData[];
	settingsData: BlockValueData[];
	expose: Array<{ contentKey: string; culture: string | null; segment: string | null }>;
}

/**
 * Find a block's content data and layout entry by its content key across all property values.
 */
export function findBlockInValues(
	allValues: Array<{ alias: string; value: unknown }>,
	blockKey: string,
): { propertyAlias: string; blockValue: BlockValue; block: BlockValueData; layoutEntry: BlockValueLayout | undefined } | undefined {
	for (const val of allValues) {
		const raw = val.value as BlockValue;
		if (!raw || typeof raw !== 'object' || !Array.isArray(raw.contentData)) continue;

		const block = raw.contentData.find((b) => b.key === blockKey);
		if (block) {
			// Find the matching layout entry to access settingsKey, areas, etc.
			let layoutEntry: BlockValueLayout | undefined;
			for (const layouts of Object.values(raw.layout)) {
				layoutEntry = layouts?.find((entry) => entry.contentKey === blockKey);
				if (layoutEntry) break;
			}
			return { propertyAlias: val.alias, blockValue: raw, block, layoutEntry };
		}
	}
	return undefined;
}

/**
 * Merge new values into an existing values array (immutable).
 */
function mergeValues(
	existing: BlockValueData['values'],
	newValues: Array<{ alias: string; value: unknown }>,
): BlockValueData['values'] {
	const merged = [...existing];
	for (const nv of newValues) {
		const idx = merged.findIndex((v) => v.alias === nv.alias);
		if (idx >= 0) {
			merged[idx] = { ...merged[idx], value: nv.value };
		} else {
			merged.push({ alias: nv.alias, value: nv.value });
		}
	}
	return merged;
}

/**
 * Update a block's property values within the block value structure.
 * Returns a new BlockValue with the updated content data (immutable).
 */
export function updateBlockPropertyValues(
	blockValue: BlockValue,
	blockKey: string,
	newValues: Array<{ alias: string; value: unknown }>,
): BlockValue {
	return {
		...blockValue,
		contentData: blockValue.contentData.map((block) => {
			if (block.key !== blockKey) return block;
			return { ...block, values: mergeValues(block.values, newValues) };
		}),
	};
}

/**
 * Update a block's settings values within the block value structure.
 * Returns a new BlockValue with the updated settings data (immutable).
 */
export function updateBlockSettingsValues(
	blockValue: BlockValue,
	settingsKey: string,
	newValues: Array<{ alias: string; value: unknown }>,
): BlockValue {
	return {
		...blockValue,
		settingsData: blockValue.settingsData.map((settings) => {
			if (settings.key !== settingsKey) return settings;
			return { ...settings, values: mergeValues(settings.values, newValues) };
		}),
	};
}

/**
 * Reorder a block within the layout array.
 * Returns a new BlockValue with the block moved (immutable).
 */
export function reorderBlockInValue(blockValue: BlockValue, blockKey: string, toIndex: number): BlockValue {
	const layoutKey = Object.keys(blockValue.layout)[0];
	if (!layoutKey) return blockValue;

	const existingLayout = blockValue.layout[layoutKey];
	if (!existingLayout) return blockValue;

	const fromIndex = existingLayout.findIndex((entry) => entry.contentKey === blockKey);
	if (fromIndex === -1 || fromIndex === toIndex) return blockValue;

	const newLayout = [...existingLayout];
	const [moved] = newLayout.splice(fromIndex, 1);
	newLayout.splice(toIndex, 0, moved);

	return {
		...blockValue,
		layout: { ...blockValue.layout, [layoutKey]: newLayout },
	};
}

/**
 * Ensure a block has a settings entry in the value structure.
 * If the layout entry has no settingsKey, creates a new settings data entry and
 * sets the settingsKey on the layout. Returns the updated value and the settingsKey.
 */
export function ensureBlockSettings(
	blockValue: BlockValue,
	blockKey: string,
	settingsElementTypeKey: string,
): { updatedValue: BlockValue; settingsKey: string } {
	const layoutKey = Object.keys(blockValue.layout)[0];
	if (!layoutKey) throw new Error('No layout key found');

	const existingLayout = blockValue.layout[layoutKey] ?? [];
	const layoutEntry = existingLayout.find((entry) => entry.contentKey === blockKey);
	if (!layoutEntry) throw new Error(`No layout entry for block ${blockKey}`);

	// Already has settings — return as-is
	if (layoutEntry.settingsKey) {
		return { updatedValue: blockValue, settingsKey: layoutEntry.settingsKey as string };
	}

	const settingsKey = crypto.randomUUID();

	return {
		updatedValue: {
			...blockValue,
			layout: {
				...blockValue.layout,
				[layoutKey]: existingLayout.map((entry) =>
					entry.contentKey === blockKey ? { ...entry, settingsKey } : entry,
				),
			},
			settingsData: [
				...blockValue.settingsData,
				{
					key: settingsKey,
					contentTypeKey: settingsElementTypeKey,
					values: [],
				},
			],
		},
		settingsKey,
	};
}

/**
 * Remove a block from the value structure by its content key.
 * Returns a new BlockValue with the block removed (immutable).
 */
export function removeBlockFromValue(blockValue: BlockValue, blockKey: string): BlockValue {
	const layoutKey = Object.keys(blockValue.layout)[0];
	const existingLayout = layoutKey ? (blockValue.layout[layoutKey] ?? []) : [];

	// Find the layout entry (may be nested in grid areas) to get the settingsKey before removing
	const layoutEntry = findLayoutEntryRecursive(existingLayout, blockKey);
	const settingsKey = layoutEntry?.settingsKey;

	const newLayout = removeLayoutEntryRecursive(existingLayout, blockKey);

	return {
		...blockValue,
		layout: layoutKey ? { ...blockValue.layout, [layoutKey]: newLayout } : blockValue.layout,
		contentData: blockValue.contentData.filter((b) => b.key !== blockKey),
		settingsData: settingsKey
			? blockValue.settingsData.filter((s) => s.key !== settingsKey)
			: blockValue.settingsData,
		expose: blockValue.expose.filter((e) => e.contentKey !== blockKey),
	};
}

/** Recursively find a layout entry by contentKey, searching through block grid areas. */
function findLayoutEntryRecursive(entries: BlockValueLayout[], blockKey: string): BlockValueLayout | undefined {
	for (const entry of entries) {
		if (entry.contentKey === blockKey) return entry;
		const areas = entry.areas as Array<{ key: string; items: BlockValueLayout[] }> | undefined;
		if (areas) {
			for (const area of areas) {
				const found = findLayoutEntryRecursive(area.items, blockKey);
				if (found) return found;
			}
		}
	}
	return undefined;
}

/** Recursively remove a layout entry by contentKey, searching through block grid areas. */
function removeLayoutEntryRecursive(entries: BlockValueLayout[], blockKey: string): BlockValueLayout[] {
	return entries
		.filter((entry) => entry.contentKey !== blockKey)
		.map((entry) => {
			const areas = entry.areas as Array<{ key: string; items: BlockValueLayout[] }> | undefined;
			if (!areas) return entry;
			return {
				...entry,
				areas: areas.map((area) => ({
					...area,
					items: removeLayoutEntryRecursive(area.items, blockKey),
				})),
			};
		});
}

/**
 * Add a new block to a block value structure at the given index.
 * Returns a new BlockValue with the block added (immutable).
 */
export function addBlockToValue(
	blockValue: BlockValue,
	contentTypeKey: string,
	insertIndex: number,
	editorAlias: string = 'Umbraco.BlockList',
	blockTypeAreas?: Array<{ key: string }>,
	settingsElementTypeKey?: string,
): { updatedValue: BlockValue; contentKey: string } {
	const contentKey = crypto.randomUUID();

	// Add to contentData
	const newContentData = [
		...blockValue.contentData,
		{
			key: contentKey,
			contentTypeKey,
			values: [],
		},
	];

	// Add settings entry if the block type has a settings element type
	let newSettingsData = [...blockValue.settingsData];
	let settingsKey: string | undefined;
	if (settingsElementTypeKey) {
		settingsKey = crypto.randomUUID();
		newSettingsData = [
			...newSettingsData,
			{
				key: settingsKey,
				contentTypeKey: settingsElementTypeKey,
				values: [],
			},
		];
	}

	// Add to layout at the insert index
	const layoutKey = Object.keys(blockValue.layout)[0] ?? editorAlias;
	const existingLayout = blockValue.layout[layoutKey] ?? [];
	const newLayout = [...existingLayout];
	const layoutEntry: BlockValueLayout = { contentKey };

	if (settingsKey) {
		layoutEntry.settingsKey = settingsKey;
	}

	// Block Grid requires rowSpan and columnSpan on layout items
	if (layoutKey === UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS) {
		layoutEntry.columnSpan = 12;
		layoutEntry.rowSpan = 1;
		// Initialize areas from the block type configuration so layout blocks
		// (e.g. two-column layouts) render their area containers correctly.
		layoutEntry.areas = (blockTypeAreas ?? []).map((a) => ({ key: a.key, items: [] }));
	}

	newLayout.splice(insertIndex, 0, layoutEntry);

	// Add to expose
	const newExpose = [...blockValue.expose, { contentKey, culture: null, segment: null }];

	return {
		updatedValue: {
			...blockValue,
			layout: { ...blockValue.layout, [layoutKey]: newLayout },
			contentData: newContentData,
			settingsData: newSettingsData,
			expose: newExpose,
		},
		contentKey,
	};
}

/**
 * Add a new block to a specific area of a parent block in a block grid.
 * Finds the parent block in the layout, matches the area by alias using
 * the block type config, and inserts the new block into that area's items.
 * Returns a new BlockValue with the block added (immutable).
 */
export function addBlockToArea(
	blockValue: BlockValue,
	parentBlockKey: string,
	areaAlias: string,
	contentTypeKey: string,
	insertIndex: number,
	areaConfigs: Array<{ key: string; alias: string }>,
): { updatedValue: BlockValue; contentKey: string } {
	const contentKey = crypto.randomUUID();
	const layoutKey = Object.keys(blockValue.layout)[0];
	if (!layoutKey) throw new Error('No layout key found');

	// Find the area config key from the alias
	const areaConfig = areaConfigs.find((a) => a.alias === areaAlias);
	if (!areaConfig) throw new Error(`No area config found for alias "${areaAlias}"`);

	// Add to contentData
	const newContentData = [...blockValue.contentData, { key: contentKey, contentTypeKey, values: [] }];

	// Add to expose
	const newExpose = [...blockValue.expose, { contentKey, culture: null, segment: null }];

	// Deep-update the layout: find parent block, find area by key, insert item
	const newLayout = (blockValue.layout[layoutKey] ?? []).map((entry) => {
		if (entry.contentKey !== parentBlockKey) return entry;

		const areas = ((entry.areas as Array<{ key: string; items: BlockValueLayout[] }>) ?? []).map((area) => {
			if (area.key !== areaConfig.key) return area;
			const newItems = [...area.items];
			const newItem: BlockValueLayout = { contentKey, columnSpan: 12, rowSpan: 1 };
			newItems.splice(insertIndex, 0, newItem);
			return { ...area, items: newItems };
		});

		return { ...entry, areas };
	});

	return {
		updatedValue: {
			...blockValue,
			layout: { ...blockValue.layout, [layoutKey]: newLayout },
			contentData: newContentData,
			expose: newExpose,
		},
		contentKey,
	};
}

/**
 * Move a block from its current position to a new position.
 * Supports moving between root layout, areas, and across different areas.
 *
 * Target is specified by `targetParentBlockKey` + `targetAreaAlias`:
 * - Both null/undefined → insert into root layout
 * - Both set → insert into a specific area of a parent block
 *
 * @param areaConfigs - Area configs from the block type, needed to map alias → key
 *   when the target is an area. Not needed when moving to root.
 */
export function moveBlock(
	blockValue: BlockValue,
	blockKey: string,
	targetIndex: number,
	targetParentBlockKey?: string,
	targetAreaAlias?: string,
	areaConfigs?: Array<{ key: string; alias: string }>,
): BlockValue {
	const layoutKey = Object.keys(blockValue.layout)[0];
	if (!layoutKey) return blockValue;

	const layout = blockValue.layout[layoutKey] ?? [];

	// --- Step 1: Remove the block from its current position ---

	let movedEntry: BlockValueLayout | undefined;

	// Try removing from root layout
	const rootIndex = layout.findIndex((entry) => entry.contentKey === blockKey);
	let newLayout: BlockValueLayout[];

	if (rootIndex !== -1) {
		movedEntry = layout[rootIndex];
		newLayout = layout.filter((_, i) => i !== rootIndex);
	} else {
		// Search in areas of all layout items
		newLayout = layout.map((entry) => {
			if (movedEntry) return entry; // Already found
			const areas = (entry.areas as Array<{ key: string; items: BlockValueLayout[] }>) ?? [];
			const updatedAreas = areas.map((area) => {
				if (movedEntry) return area;
				const idx = area.items.findIndex((item) => item.contentKey === blockKey);
				if (idx === -1) return area;
				movedEntry = area.items[idx];
				return { ...area, items: area.items.filter((_, i) => i !== idx) };
			});
			return { ...entry, areas: updatedAreas };
		});
	}

	if (!movedEntry) return blockValue; // Block not found

	// --- Step 2: Insert at the target position ---

	if (targetParentBlockKey && targetAreaAlias && areaConfigs) {
		// Insert into a specific area
		const areaConfig = areaConfigs.find((a) => a.alias === targetAreaAlias);
		if (!areaConfig) return blockValue;

		newLayout = newLayout.map((entry) => {
			if (entry.contentKey !== targetParentBlockKey) return entry;
			const areas = ((entry.areas as Array<{ key: string; items: BlockValueLayout[] }>) ?? []).map((area) => {
				if (area.key !== areaConfig.key) return area;
				const newItems = [...area.items];
				newItems.splice(targetIndex, 0, movedEntry!);
				return { ...area, items: newItems };
			});
			return { ...entry, areas };
		});
	} else {
		// Insert into root layout
		newLayout.splice(targetIndex, 0, movedEntry);
	}

	return {
		...blockValue,
		layout: { ...blockValue.layout, [layoutKey]: newLayout },
	};
}

/**
 * Merge a pasted block value into an existing block value at the given index.
 * Appends all contentData, settingsData, and expose entries, and inserts
 * the pasted layout entries at the specified position.
 */
export function mergeBlockValueInto(
	target: BlockValue,
	pasted: BlockValue,
	insertIndex: number,
): BlockValue {
	const layoutKey = Object.keys(target.layout)[0] ?? Object.keys(pasted.layout)[0] ?? 'Umbraco.BlockList';
	const existingLayout = target.layout[layoutKey] ?? [];
	const pastedLayout = pasted.layout[layoutKey] ?? [];

	const newLayout = [...existingLayout];
	const clampedIndex = Math.min(Math.max(insertIndex, 0), newLayout.length);
	newLayout.splice(clampedIndex, 0, ...pastedLayout);

	return {
		layout: { ...target.layout, [layoutKey]: newLayout },
		contentData: [...target.contentData, ...pasted.contentData],
		settingsData: [...target.settingsData, ...pasted.settingsData],
		expose: [...target.expose, ...pasted.expose],
	};
}
