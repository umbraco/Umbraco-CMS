import type { UmbMockDataSet, UmbMockDataKey, UmbMockDataKeyMap } from '../types/mock-data-set.types.js';

export const UMB_MOCK_SET_NAME = import.meta.env.VITE_MOCK_SET || 'default';

// Dynamically discover all mock data sets using Vite's glob imports
const mockSets = import.meta.glob<UmbMockDataSet>('./**/index.ts');

const loadSet = async (): Promise<UmbMockDataSet> => {
	const path = `./${UMB_MOCK_SET_NAME}/index.ts`;

	if (path in mockSets) {
		return mockSets[path]() as Promise<UmbMockDataSet>;
	}

	console.warn(`Mock set "${UMB_MOCK_SET_NAME}" not found, falling back to "default"`);
	return mockSets['./default/index.ts']() as Promise<UmbMockDataSet>;
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
