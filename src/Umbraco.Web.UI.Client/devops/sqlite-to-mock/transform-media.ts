/**
 * Transform media records into mock data format.
 */
import { prepare, ObjectTypes, formatGuid, writeDataFile } from './db.js';

interface MediaRow {
	nodeId: number;
	uniqueId: string;
	parentId: number;
	level: number;
	path: string;
	sortOrder: number;
	trashed: number;
	text: string | null;
	createDate: string;
	contentTypeNodeId: number;
	contentTypeAlias: string;
	contentTypeIcon: string | null;
	listView: string | null;
}

interface ContentVersion {
	id: number;
	nodeId: number;
	versionDate: string;
	userId: number | null;
	current: number;
	text: string | null;
}

interface PropertyData {
	versionId: number;
	propertyTypeId: number;
	languageId: number | null;
	segment: string | null;
	intValue: number | null;
	decimalValue: number | null;
	dateValue: string | null;
	varcharValue: string | null;
	textValue: string | null;
}

interface PropertyTypeInfo {
	id: number;
	alias: string;
	contentTypeId: number;
	editorAlias: string;
}

export function transformMedia(): void {
	// Query media with their content type information
	const query = prepare(`
		SELECT
			n.id as nodeId, n.uniqueId, n.parentId, n.level, n.path, n.sortOrder,
			n.trashed, n.text, n.createDate,
			c.contentTypeId as contentTypeNodeId,
			ct.alias as contentTypeAlias,
			ct.icon as contentTypeIcon,
			ct.listView
		FROM umbracoNode n
		JOIN umbracoContent c ON n.id = c.nodeId
		JOIN cmsContentType ct ON c.contentTypeId = ct.nodeId
		WHERE n.nodeObjectType = ?
		ORDER BY n.level, n.sortOrder
	`);

	const rows = query.all(ObjectTypes.Media) as MediaRow[];

	// Get current content versions
	const versionQuery = prepare(`
		SELECT id, nodeId, versionDate, userId, current, text
		FROM umbracoContentVersion
		WHERE current = 1
	`);

	const versions = versionQuery.all() as ContentVersion[];
	const versionMap = new Map<number, ContentVersion>();
	for (const v of versions) {
		versionMap.set(v.nodeId, v);
	}

	// Get property data for current versions
	const propertyDataQuery = prepare(`
		SELECT
			pd.versionId, pd.propertyTypeId, pd.languageId, pd.segment,
			pd.intValue, pd.decimalValue, pd.dateValue, pd.varcharValue, pd.textValue
		FROM umbracoPropertyData pd
		JOIN umbracoContentVersion cv ON pd.versionId = cv.id
		WHERE cv.current = 1
	`);

	const propertyData = propertyDataQuery.all() as PropertyData[];
	const propertyDataByVersion = new Map<number, PropertyData[]>();
	for (const pd of propertyData) {
		const list = propertyDataByVersion.get(pd.versionId) || [];
		list.push(pd);
		propertyDataByVersion.set(pd.versionId, list);
	}

	// Get property type information
	// Note: Use column aliases to normalize PascalCase column names to camelCase
	const propertyTypeQuery = prepare(`
		SELECT
			pt.id, pt.Alias as alias, pt.contentTypeId,
			dt.propertyEditorAlias as editorAlias
		FROM cmsPropertyType pt
		JOIN umbracoDataType dt ON pt.dataTypeId = dt.nodeId
	`);

	const propertyTypes = propertyTypeQuery.all() as PropertyTypeInfo[];
	const propertyTypeMap = new Map<number, PropertyTypeInfo>();
	for (const pt of propertyTypes) {
		propertyTypeMap.set(pt.id, pt);
	}

	// Get media type nodes for ID lookup
	const mediaTypeNodeQuery = prepare(`
		SELECT id, uniqueId
		FROM umbracoNode
		WHERE nodeObjectType = ?
	`);

	const mediaTypeNodes = mediaTypeNodeQuery.all(ObjectTypes.MediaType) as { id: number; uniqueId: string }[];
	const mediaTypeIdMap = new Map<number, string>();
	for (const mt of mediaTypeNodes) {
		mediaTypeIdMap.set(mt.id, formatGuid(mt.uniqueId));
	}

	// Build parent lookup map (nodeId -> uniqueId)
	const nodeIdMap = new Map<number, string>();
	for (const row of rows) {
		nodeIdMap.set(row.nodeId, formatGuid(row.uniqueId));
	}

	// Check if media items have children
	const hasChildrenMap = new Map<number, boolean>();
	for (const row of rows) {
		if (row.parentId > 0) {
			hasChildrenMap.set(row.parentId, true);
		}
	}

	// Transform media
	const media = rows
		.filter((row) => row.trashed === 0)
		.map((row) => {
			const version = versionMap.get(row.nodeId);
			const parentId = row.parentId > 0 ? nodeIdMap.get(row.parentId) : null;

			// Get property values for this media's version
			const versionPropertyData = version ? propertyDataByVersion.get(version.id) || [] : [];

			// Transform property values
			const values = versionPropertyData.map((pd) => {
				const propType = propertyTypeMap.get(pd.propertyTypeId);
				if (!propType) return null;

				// Determine the value based on what's populated
				let value: unknown = null;
				if (pd.textValue !== null) {
					// Try to parse as JSON first (for image cropper data, etc.)
					try {
						value = JSON.parse(pd.textValue);
					} catch {
						value = pd.textValue;
					}
				} else if (pd.varcharValue !== null) {
					value = pd.varcharValue;
				} else if (pd.intValue !== null) {
					value = pd.intValue;
				} else if (pd.decimalValue !== null) {
					value = pd.decimalValue;
				} else if (pd.dateValue !== null) {
					value = pd.dateValue;
				}

				return {
					editorAlias: propType.editorAlias,
					alias: propType.alias,
					value,
				};
			}).filter((v): v is NonNullable<typeof v> => v !== null);

			// Build media type info
			const mediaTypeInfo: Record<string, unknown> = {
				id: mediaTypeIdMap.get(row.contentTypeNodeId) || `unknown-${row.contentTypeNodeId}`,
				icon: row.contentTypeIcon || 'icon-picture',
			};

			// Add collection if it has a list view
			if (row.listView) {
				mediaTypeInfo.collection = { id: row.listView.toLowerCase() };
			}

			return {
				hasChildren: hasChildrenMap.get(row.nodeId) || false,
				id: formatGuid(row.uniqueId),
				createDate: row.createDate,
				parent: parentId ? { id: parentId } : null,
				noAccess: false,
				isTrashed: false,
				mediaType: mediaTypeInfo,
				values,
				variants: [
					{
						publishDate: version?.versionDate || row.createDate,
						culture: null,
						segment: null,
						name: row.text || 'Unnamed',
						createDate: row.createDate,
						updateDate: version?.versionDate || row.createDate,
					},
				],
				flags: [] as string[],
			};
		});

	// Generate TypeScript content
	const content = `import type { UmbMockMediaModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockMediaModel> = ${JSON.stringify(media, null, '\t')};
`;

	writeDataFile('media.data.ts', content);
	console.log(`Transformed ${media.length} media items`);
}

// Run if called directly
transformMedia();
