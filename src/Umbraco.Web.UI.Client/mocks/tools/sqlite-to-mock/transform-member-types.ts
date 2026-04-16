/**
 * Transform cmsContentType records (member types) into mock data format.
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

interface Composition {
	parentContentTypeId: number;
	childContentTypeId: number;
}

interface MemberTypePropertyMeta {
	propertytypeId: number;
	memberCanEdit: number;
	viewOnProfile: number;
	isSensitive: number;
}

export function transformMemberTypes(): void {
	// Query member types with their node and content type information
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

	const rows = query.all(ObjectTypes.MemberType) as ContentTypeRow[];

	// Get folders
	const folderQuery = prepare(`
		SELECT id, uniqueId, parentId, level, path, sortOrder, text
		FROM umbracoNode
		WHERE nodeObjectType = ?
		  AND trashed = 0
		ORDER BY sortOrder
	`);

	const folders = folderQuery.all(ObjectTypes.MemberTypeContainer) as UmbracoNode[];

	// Get property types
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

	// Get member-specific property metadata (isSensitive, visibility)
	const memberTypeQuery = prepare(`
		SELECT propertytypeId, memberCanEdit, viewOnProfile, isSensitive
		FROM cmsMemberType
	`);

	const memberTypeMeta = memberTypeQuery.all() as MemberTypePropertyMeta[];
	const memberPropertyMap = new Map<number, MemberTypePropertyMeta>();
	for (const m of memberTypeMeta) {
		memberPropertyMap.set(m.propertytypeId, m);
	}

	// Get compositions (inheritance relationships)
	const compositionQuery = prepare(`
		SELECT parentContentTypeId, childContentTypeId
		FROM cmsContentType2ContentType
	`);

	const compositions = compositionQuery.all() as Composition[];

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

	const compositionsByContentType = new Map<number, Composition[]>();
	for (const c of compositions) {
		const list = compositionsByContentType.get(c.childContentTypeId) || [];
		list.push(c);
		compositionsByContentType.set(c.childContentTypeId, list);
	}

	// Check if a member type has children
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

	// Transform member types
	const memberTypes = rows.map((row) => {
		const variations = getVariationFlags(row.variations);
		const parentId = row.parentId > 0 ? parentMap.get(row.parentId) : null;
		const ctGroups = groupsByContentType.get(row.id) || [];
		const ctProperties = propertyTypesByContentType.get(row.id) || [];
		const ctCompositions = compositionsByContentType.get(row.id) || [];

		// Transform properties
		const properties = ctProperties.map((pt) => {
			const ptVariations = getVariationFlags(pt.variations);
			const containerId = pt.propertyTypeGroupId ? groupIdMap.get(pt.propertyTypeGroupId) : null;
			const memberProp = memberPropertyMap.get(pt.id);

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
				isSensitive: memberProp?.isSensitive === 1,
				visibility: {
					memberCanView: memberProp?.viewOnProfile === 1,
					memberCanEdit: memberProp?.memberCanEdit === 1,
				},
			};
		});

		// Transform containers (groups/tabs)
		// Type: 0 = Group, 1 = Tab (from PropertyGroupType enum in Umbraco.Core)
		const containers = ctGroups.map((g) => {
			return {
				id: formatGuid(g.uniqueId),
				parent: null,
				name: g.text || 'Unnamed',
				type: g.type === 0 ? 'Group' : 'Tab',
				sortOrder: g.sortOrder,
			};
		});

		// Transform compositions
		const compositionsList = ctCompositions.map((c) => ({
			memberType: { id: parentMap.get(c.parentContentTypeId) || `unknown-${c.parentContentTypeId}` },
			compositionType: row.parentId === c.parentContentTypeId ? 'Inheritance' : 'Composition',
		}));

		return {
			name: row.text || 'Unnamed',
			id: formatGuid(row.uniqueId),
			description: row.description,
			alias: row.alias,
			icon: row.icon || 'icon-user',
			allowedAsRoot: row.allowAtRoot === 1,
			variesByCulture: variations.variesByCulture,
			variesBySegment: variations.variesBySegment,
			isElement: row.isElement === 1,
			properties,
			containers,
			compositions: compositionsList,
			parent: parentId ? { id: parentId } : null,
			hasChildren: hasChildrenMap.get(row.id) || false,
			hasListView: !!row.listView,
			isFolder: false,
			collection: row.listView ? { id: row.listView.toLowerCase() } : undefined,
			flags: [] as string[],
		};
	});

	// Transform folders
	const folderData = folders.map((folder) => {
		const parentId = folder.parentId > 0 ? parentMap.get(folder.parentId) : null;

		return {
			name: folder.text || 'Unnamed Folder',
			id: formatGuid(folder.uniqueId),
			description: null,
			alias: (folder.text || 'folder').toLowerCase().replace(/\s+/g, ''),
			icon: 'icon-folder',
			allowedAsRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			properties: [],
			containers: [],
			compositions: [],
			parent: parentId ? { id: parentId } : null,
			hasChildren: hasChildrenMap.get(folder.id) || false,
			hasListView: false,
			isFolder: true,
			flags: [] as string[],
		};
	});

	const allMemberTypes = [...folderData, ...memberTypes];

	// Generate TypeScript content
	const content = `import type { UmbMockMemberTypeModel } from '../../mock-data-set.types.js';
import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string composition type to enum
function mapCompositionType(type: string): CompositionTypeModel {
	switch (type) {
		case 'Composition': return CompositionTypeModel.COMPOSITION;
		case 'Inheritance': return CompositionTypeModel.INHERITANCE;
		default: return CompositionTypeModel.COMPOSITION;
	}
}

const rawData: Array<Omit<UmbMockMemberTypeModel, 'compositions'> & { compositions: Array<{ memberType: { id: string }; compositionType: string }> }> = ${JSON.stringify(allMemberTypes, null, '\t')};

export const data: Array<UmbMockMemberTypeModel> = rawData.map(mt => ({
	...mt,
	compositions: mt.compositions.map(c => ({
		...c,
		compositionType: mapCompositionType(c.compositionType),
	})),
}));
`;

	writeDataFile('member-type.data.ts', content);
	console.log(`Transformed ${memberTypes.length} member types and ${folders.length} folders`);
}
