import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import type { UmbDynamicRoot } from '@umbraco-cms/backoffice/dynamic-root';

interface UmbContentPickerStartNode {
	type?: string | null;
	id?: string | null;
	dynamicRoot?: UmbDynamicRoot | null;
}

interface UmbContentPickerConvertibleConfig {
	entityType: string;
	startNodeId?: string;
	dynamicRoot?: UmbDynamicRoot;
	filter?: string;
}

/**
 * Reads the settings of the Multi Node Tree Picker that the dedicated pickers name differently.
 * @param {UmbPropertyEditorConfig} config - The Multi Node Tree Picker settings.
 * @returns {UmbContentPickerConvertibleConfig} The entity type it picks, which is documents when unset, its start node and its filter.
 */
export function readContentPickerConfig(config: UmbPropertyEditorConfig): UmbContentPickerConvertibleConfig {
	const startNode = config.find((x) => x.alias === 'startNode')?.value as UmbContentPickerStartNode | undefined;
	const filter = config.find((x) => x.alias === 'filter')?.value as string | undefined;

	return {
		entityType: startNode?.type || 'content',
		startNodeId: startNode?.id || undefined,
		dynamicRoot: startNode?.dynamicRoot ?? undefined,
		filter: filter || undefined,
	};
}

/**
 * Keeps only the settings that have a value.
 * @param {UmbPropertyEditorConfig} config - The settings.
 * @returns {UmbPropertyEditorConfig} The settings that have a value.
 */
export function withValues(config: UmbPropertyEditorConfig): UmbPropertyEditorConfig {
	return config.filter((x) => x.value !== undefined);
}
