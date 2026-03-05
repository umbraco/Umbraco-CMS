/**
 * Transform umbracoDataType records into mock data format.
 */
import {
	prepare,
	ObjectTypes,
	formatGuid,
	getEditorUiAlias,
	parseConfig,
	writeDataFile,
	type UmbracoNode,
	type DataType,
} from './db.js';

interface DataTypeRow extends UmbracoNode, DataType {}

export function transformDataTypes(): void {
	// Query data types with their node information
	const query = prepare(`
		SELECT
			n.id, n.uniqueId, n.parentId, n.level, n.path, n.sortOrder,
			n.trashed, n.nodeUser, n.text, n.nodeObjectType, n.createDate,
			dt.propertyEditorAlias, dt.dbType, dt.config
		FROM umbracoNode n
		JOIN umbracoDataType dt ON n.id = dt.nodeId
		WHERE n.nodeObjectType = ?
		  AND n.trashed = 0
		ORDER BY n.sortOrder
	`);

	const rows = query.all(ObjectTypes.DataType) as DataTypeRow[];

	// Also get data type folders
	const folderQuery = prepare(`
		SELECT
			n.id, n.uniqueId, n.parentId, n.level, n.path, n.sortOrder,
			n.trashed, n.nodeUser, n.text, n.nodeObjectType, n.createDate
		FROM umbracoNode n
		WHERE n.nodeObjectType = ?
		  AND n.trashed = 0
		ORDER BY n.sortOrder
	`);

	const folders = folderQuery.all(ObjectTypes.DataTypeContainer) as UmbracoNode[];

	// Build parent lookup map (nodeId -> uniqueId)
	const parentMap = new Map<number, string>();
	for (const row of [...rows, ...folders]) {
		parentMap.set(row.id, formatGuid(row.uniqueId));
	}

	// Transform data types
	const dataTypes = rows.map((row) => {
		const config = parseConfig(row.config);
		const parentId = row.parentId > 0 ? parentMap.get(row.parentId) : null;

		return {
			name: row.text || 'Unnamed',
			id: formatGuid(row.uniqueId),
			parent: parentId ? { id: parentId } : null,
			editorAlias: row.propertyEditorAlias,
			editorUiAlias: getEditorUiAlias(row.propertyEditorAlias),
			hasChildren: false,
			isFolder: false,
			isDeletable: true,
			canIgnoreStartNodes: false,
			flags: [] as string[],
			values: config,
		};
	});

	// Transform folders
	const folderData = folders.map((folder) => {
		// Check if folder has children
		const hasChildren = rows.some((r) => r.parentId === folder.id) || folders.some((f) => f.parentId === folder.id);
		const parentId = folder.parentId > 0 ? parentMap.get(folder.parentId) : null;

		return {
			name: folder.text || 'Unnamed Folder',
			id: formatGuid(folder.uniqueId),
			parent: parentId ? { id: parentId } : null,
			editorAlias: '',
			editorUiAlias: '',
			hasChildren,
			isFolder: true,
			isDeletable: true,
			canIgnoreStartNodes: false,
			flags: [] as string[],
			values: [] as Record<string, unknown>[],
		};
	});

	// Combine folders and data types
	const allDataTypes = [...folderData, ...dataTypes];

	// Generate TypeScript content
	const content = `import type { UmbMockDataTypeModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockDataTypeModel> = ${JSON.stringify(allDataTypes, null, '\t')};
`;

	writeDataFile('data-type.data.ts', content);
	console.log(`Transformed ${dataTypes.length} data types and ${folders.length} folders`);
}
