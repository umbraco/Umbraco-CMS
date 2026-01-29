import type { UmbMockDataSet, UmbMockDataKey, UmbMockDataKeyMap } from '../types/mock-data-set.types.js';

export const UMB_MOCK_SET_NAME = import.meta.env.VITE_MOCK_SET || 'default';

const loadSet = async (): Promise<UmbMockDataSet> => {
	switch (UMB_MOCK_SET_NAME) {
		case 'test':
			return import('./test/index.js') as Promise<UmbMockDataSet>;
		case 'kenn':
			return import('./kenn/index.js') as Promise<UmbMockDataSet>;
		default:
			return import('./default/index.js') as Promise<UmbMockDataSet>;
	}
};

/**
 * Type-safe async accessor for mock data.
 * @param key
 * @example
 * ```typescript
 * // Returns Array<UmbMockDataTypeModel>
 * const dataTypes = await getMockData('dataType');
 *
 * // Returns Array<UmbMockTemplateModel>
 * const templates = await getMockData('template');
 * ```
 */
export async function getMockData<K extends UmbMockDataKey>(key: K): Promise<UmbMockDataKeyMap[K]> {
	const set = await loadSet();
	return set[key];
}

/**
 * Gets the entire mock data set.
 * Prefer getMockData() for individual data types when possible.
 */
export async function getDataSet(): Promise<UmbMockDataSet> {
	return loadSet();
}

// Legacy export for backward compatibility during migration
// This uses top-level await (existing behavior)
export const dataSet = await loadSet();

console.log(`[MSW] Using mock data set: "${UMB_MOCK_SET_NAME}"`);

// Re-export types for convenience
export type {
	UmbMockDataSet,
	UmbMockDataKey,
	UmbMockDataKeyMap,
	UmbMockDataTypeModel,
	UmbMockDictionaryModel,
	UmbMockDocumentModel,
	UmbMockDocumentBlueprintModel,
	UmbMockDocumentTypeModel,
	UmbMockLanguageModel,
	UmbMockMediaModel,
	UmbMockMediaTypeModel,
	UmbMockMemberModel,
	UmbMockMemberGroupModel,
	UmbMockMemberTypeModel,
	UmbMockPartialViewModel,
	UmbMockRelationModel,
	UmbMockRelationTypeModel,
	UmbMockRelationTypeItemModel,
	UmbMockScriptModel,
	UmbMockStaticFileModel,
	UmbMockStylesheetModel,
	UmbMockTemplateModel,
	UmbMockUserModel,
	UmbMockUserGroupModel,
	UmbMockTrackedReferenceItemModel,
	UmbMockLogLevelsModel,
} from '../types/mock-data-set.types.js';
