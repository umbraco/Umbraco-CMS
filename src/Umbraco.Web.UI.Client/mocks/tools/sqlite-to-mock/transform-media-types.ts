/**
 * Transform cmsContentType records (media types) into mock data format.
 */
import {
	prepare,
	ObjectTypes,
	formatGuid,
	getVariationFlags,
	writeDataFile,
	type UmbracoNode,
	type ContentType,
	type PropertyType,
} from './db.js';

// Local interface matching actual DB schema
interface PropertyTypeGroup {
	id: number;
	uniqueId: string;
	contentTypeNodeId: number;
	type: number;
	text: string | null;
	alias: string | null;
	sortOrder: number;
}

interface ContentTypeRow extends UmbracoNode, ContentType {}

interface AllowedContentType {
	childContentTypeId: number;
	sortOrder: number;
}

export function transformMediaTypes(): void {
	// Query media types with their node and content type information
	const query = prepare(`
		SELECT
			n.id, n.uniqueId, n.parentId, n.level, n.path, n.sortOrder,
			n.trashed, n.nodeUser, n.text, n.nodeObjectType, n.createDate,
			ct.alias, ct.icon, ct.thumbnail, ct.description, ct.listView,
			ct.allowAtRoot, ct.variations, ct.isElement
		FROM umbracoNode n
		JOIN cmsContentType ct ON n.id = ct.nodeId
		WHERE n.nodeObjectType = ?
		  AND n.trashed = 0
		ORDER BY n.sortOrder
	`);

	const rows = query.all(ObjectTypes.MediaType) as ContentTypeRow[];

	// Get folders
	const folderQuery = prepare(`
		SELECT id, uniqueId, parentId, level, path, sortOrder, text
		FROM umbracoNode
		WHERE nodeObjectType = ?
		  AND trashed = 0
		ORDER BY sortOrder
	`);

	const folders = folderQuery.all(ObjectTypes.MediaTypeContainer) as UmbracoNode[];

	// Get property types
	// Note: Use column aliases to normalize PascalCase column names to camelCase
	const propertyQuery = prepare(`
		SELECT
			pt.id, pt.dataTypeId, pt.contentTypeId, pt.propertyTypeGroupId,
			pt.Alias as alias, pt.Name as name, pt.sortOrder, pt.mandatory, pt.mandatoryMessage,
			pt.validationRegExp, pt.validationRegExpMessage, pt.Description as description,
			pt.variations, pt.labelOnTop
		FROM cmsPropertyType pt
		ORDER BY pt.contentTypeId, pt.sortOrder
	`);

	const propertyTypes = propertyQuery.all() as PropertyType[];

	// Get property type groups (containers/tabs)
	const groupQuery = prepare(`
		SELECT
			id, uniqueID as uniqueId, contenttypeNodeId as contentTypeNodeId, type, text, alias, sortorder as sortOrder
		FROM cmsPropertyTypeGroup
		ORDER BY contenttypeNodeId, sortorder
	`);

	const groups = groupQuery.all() as PropertyTypeGroup[];

	// Get allowed media types
	// Note: Use column aliases to normalize PascalCase column names to camelCase
	const allowedQuery = prepare(`
		SELECT Id as parentContentTypeId, AllowedId as childContentTypeId, SortOrder as sortOrder
		FROM cmsContentTypeAllowedContentType
	`);

	const allowed = allowedQuery.all() as (AllowedContentType & { parentContentTypeId: number })[];

	// Get data type nodes for ID lookup
	const dataTypeNodeQuery = prepare(`
		SELECT id, uniqueId
		FROM umbracoNode
		WHERE nodeObjectType = ?
	`);

	const dataTypeNodes = dataTypeNodeQuery.all(ObjectTypes.DataType) as { id: number; uniqueId: string }[];
	const dataTypeIdMap = new Map<number, string>();
	for (const dt of dataTypeNodes) {
		dataTypeIdMap.set(dt.id, formatGuid(dt.uniqueId));
	}

	// Build parent lookup map (nodeId -> uniqueId)
	const parentMap = new Map<number, string>();
	for (const row of [...rows, ...folders]) {
		parentMap.set(row.id, formatGuid(row.uniqueId));
	}

	// Group data by content type
	const propertyTypesByContentType = new Map<number, PropertyType[]>();
	for (const pt of propertyTypes) {
		const list = propertyTypesByContentType.get(pt.contentTypeId) || [];
		list.push(pt);
		propertyTypesByContentType.set(pt.contentTypeId, list);
	}

	const groupsByContentType = new Map<number, PropertyTypeGroup[]>();
	for (const g of groups) {
		const list = groupsByContentType.get(g.contentTypeNodeId) || [];
		list.push(g);
		groupsByContentType.set(g.contentTypeNodeId, list);
	}

	const allowedByContentType = new Map<number, AllowedContentType[]>();
	for (const a of allowed) {
		const list = allowedByContentType.get(a.parentContentTypeId) || [];
		list.push(a);
		allowedByContentType.set(a.parentContentTypeId, list);
	}

	// Check if a media type has children
	const hasChildrenMap = new Map<number, boolean>();
	for (const row of rows) {
		if (row.parentId > 0) {
			hasChildrenMap.set(row.parentId, true);
		}
	}
	for (const folder of folders) {
		if (folder.parentId > 0) {
			hasChildrenMap.set(folder.parentId, true);
		}
	}

	// Build group ID map (group.id -> group.uniqueId)
	const groupIdMap = new Map<number, string>();
	for (const g of groups) {
		groupIdMap.set(g.id, formatGuid(g.uniqueId));
	}

	// Transform media types
	const mediaTypes = rows.map((row) => {
		const variations = getVariationFlags(row.variations);
		const parentId = row.parentId > 0 ? parentMap.get(row.parentId) : null;
		const ctGroups = groupsByContentType.get(row.id) || [];
		const ctProperties = propertyTypesByContentType.get(row.id) || [];
		const ctAllowed = allowedByContentType.get(row.id) || [];

		// Transform properties
		const properties = ctProperties.map((pt) => {
			const ptVariations = getVariationFlags(pt.variations);
			const containerId = pt.propertyTypeGroupId ? groupIdMap.get(pt.propertyTypeGroupId) : null;

			return {
				id: `pt-${pt.id}`,
				container: containerId ? { id: containerId } : null,
				alias: pt.alias,
				name: pt.name,
				description: pt.description,
				dataType: { id: dataTypeIdMap.get(pt.dataTypeId) || `unknown-${pt.dataTypeId}` },
				variesByCulture: ptVariations.variesByCulture,
				variesBySegment: ptVariations.variesBySegment,
				sortOrder: pt.sortOrder,
				validation: {
					mandatory: pt.mandatory === 1,
					mandatoryMessage: pt.mandatoryMessage,
					regEx: pt.validationRegExp,
					regExMessage: pt.validationRegExpMessage,
				},
				appearance: {
					labelOnTop: pt.labelOnTop === 1,
				},
			};
		});

		// Transform containers (groups/tabs)
		const containers = ctGroups.map((g) => {
			return {
				id: formatGuid(g.uniqueId),
				parent: null,
				name: g.text || 'Unnamed',
				type: g.type === 0 ? 'Tab' : 'Group',
				sortOrder: g.sortOrder,
			};
		});

		// Transform allowed media types
		const allowedMediaTypes = ctAllowed.map((a) => ({
			mediaType: { id: parentMap.get(a.childContentTypeId) || `unknown-${a.childContentTypeId}` },
			sortOrder: a.sortOrder,
		}));

		return {
			name: row.text || 'Unnamed',
			id: formatGuid(row.uniqueId),
			parent: parentId ? { id: parentId } : null,
			description: row.description,
			alias: row.alias,
			icon: row.icon || 'icon-picture',
			flags: [] as string[],
			properties,
			containers,
			allowedAsRoot: row.allowAtRoot === 1,
			variesByCulture: variations.variesByCulture,
			variesBySegment: variations.variesBySegment,
			isElement: row.isElement === 1,
			allowedMediaTypes,
			compositions: [],
			isFolder: false,
			hasChildren: hasChildrenMap.get(row.id) || false,
			collection: row.listView ? { id: row.listView.toLowerCase() } : undefined,
			isDeletable: false,
			aliasCanBeChanged: false,
		};
	});

	// Generate TypeScript content
	const content = `import type { UmbMockMediaTypeModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockMediaTypeModel> = ${JSON.stringify(mediaTypes, null, '\t')};
`;

	writeDataFile('media-type.data.ts', content);
	console.log(`Transformed ${mediaTypes.length} media types`);
}
