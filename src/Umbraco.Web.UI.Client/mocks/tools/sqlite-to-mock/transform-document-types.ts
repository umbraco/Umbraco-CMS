/**
 * Transform cmsContentType records (document types) into mock data format.
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

interface Composition {
	childContentTypeId: number;
}

interface DocumentType {
	contentTypeNodeId: number;
	templateNodeId: number;
	IsDefault: number;
}

interface AllowedTemplate {
	contentTypeNodeId: number;
	templateNodeId: number;
}

interface TemplateNode {
	id: number;
	uniqueId: string;
}

export function transformDocumentTypes(): void {
	// Query document types with their node and content type information
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

	const rows = query.all(ObjectTypes.DocumentType) as ContentTypeRow[];

	// Get folders
	const folderQuery = prepare(`
		SELECT id, uniqueId, parentId, level, path, sortOrder, text
		FROM umbracoNode
		WHERE nodeObjectType = ?
		  AND trashed = 0
		ORDER BY sortOrder
	`);

	const folders = folderQuery.all(ObjectTypes.DocumentTypeContainer) as UmbracoNode[];

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

	// Get allowed content types
	// Note: Use column aliases to normalize PascalCase column names to camelCase
	const allowedQuery = prepare(`
		SELECT Id as parentContentTypeId, AllowedId as childContentTypeId, SortOrder as sortOrder
		FROM cmsContentTypeAllowedContentType
	`);

	const allowed = allowedQuery.all() as (AllowedContentType & { parentContentTypeId: number })[];

	// Get compositions (inheritance relationships)
	const compositionQuery = prepare(`
		SELECT parentContentTypeId, childContentTypeId
		FROM cmsContentType2ContentType
	`);

	const compositions = compositionQuery.all() as (Composition & { parentContentTypeId: number })[];

	// Get document type specific data (templates)
	const docTypeQuery = prepare(`
		SELECT contentTypeNodeId, templateNodeId, IsDefault
		FROM cmsDocumentType
	`);

	const docTypes = docTypeQuery.all() as DocumentType[];

	// Get allowed templates
	const allowedTemplateQuery = prepare(`
		SELECT contentTypeNodeId, templateNodeId
		FROM cmsDocumentType
	`);

	const allowedTemplates = allowedTemplateQuery.all() as AllowedTemplate[];

	// Get template nodes for ID lookup
	const templateNodeQuery = prepare(`
		SELECT id, uniqueId
		FROM umbracoNode
		WHERE nodeObjectType = ?
	`);

	const templateNodes = templateNodeQuery.all(ObjectTypes.Template) as TemplateNode[];
	const templateIdMap = new Map<number, string>();
	for (const t of templateNodes) {
		templateIdMap.set(t.id, formatGuid(t.uniqueId));
	}

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

	const compositionsByContentType = new Map<number, Composition[]>();
	for (const c of compositions) {
		const list = compositionsByContentType.get(c.parentContentTypeId) || [];
		list.push(c);
		compositionsByContentType.set(c.parentContentTypeId, list);
	}

	const defaultTemplateByContentType = new Map<number, number | null>();
	for (const dt of docTypes) {
		if (dt.IsDefault === 1) {
			defaultTemplateByContentType.set(dt.contentTypeNodeId, dt.templateNodeId);
		}
	}

	const allowedTemplatesByContentType = new Map<number, number[]>();
	for (const at of allowedTemplates) {
		const list = allowedTemplatesByContentType.get(at.contentTypeNodeId) || [];
		list.push(at.templateNodeId);
		allowedTemplatesByContentType.set(at.contentTypeNodeId, list);
	}

	// Check if a document type has children (other doc types or folders have it as parent)
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

	// Transform document types
	const documentTypes = rows.map((row) => {
		const variations = getVariationFlags(row.variations);
		const parentId = row.parentId > 0 ? parentMap.get(row.parentId) : null;
		const ctGroups = groupsByContentType.get(row.id) || [];
		const ctProperties = propertyTypesByContentType.get(row.id) || [];
		const ctAllowed = allowedByContentType.get(row.id) || [];
		const ctCompositions = compositionsByContentType.get(row.id) || [];
		const defaultTemplateId = defaultTemplateByContentType.get(row.id);
		const ctAllowedTemplates = allowedTemplatesByContentType.get(row.id) || [];

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

		// Transform allowed document types
		const allowedDocumentTypes = ctAllowed.map((a) => ({
			documentType: { id: parentMap.get(a.childContentTypeId) || `unknown-${a.childContentTypeId}` },
			sortOrder: a.sortOrder,
		}));

		// Transform compositions (all entries in this table are compositions)
		const compositionsList = ctCompositions.map((c) => ({
			documentType: { id: parentMap.get(c.childContentTypeId) || `unknown-${c.childContentTypeId}` },
			compositionType: 'Composition',
		}));

		// Transform allowed templates
		const allowedTemplatesList = ctAllowedTemplates
			.map((templateNodeId) => {
				const id = templateIdMap.get(templateNodeId);
				return id ? { id } : null;
			})
			.filter((t): t is { id: string } => t !== null);

		// Get default template
		const defaultTemplate = defaultTemplateId ? { id: templateIdMap.get(defaultTemplateId) || null } : null;

		return {
			allowedTemplates: allowedTemplatesList,
			defaultTemplate: defaultTemplate?.id ? defaultTemplate : null,
			id: formatGuid(row.uniqueId),
			alias: row.alias,
			name: row.text || 'Unnamed',
			description: row.description,
			icon: row.icon || 'icon-document',
			allowedAsRoot: row.allowAtRoot === 1,
			variesByCulture: variations.variesByCulture,
			variesBySegment: variations.variesBySegment,
			isElement: row.isElement === 1,
			hasChildren: hasChildrenMap.get(row.id) || false,
			parent: parentId ? { id: parentId } : null,
			isFolder: false,
			properties,
			containers,
			allowedDocumentTypes,
			compositions: compositionsList,
			cleanup: {
				preventCleanup: false,
				keepAllVersionsNewerThanDays: null,
				keepLatestVersionPerDayForDays: null,
			},
			flags: [] as string[],
			collection: row.listView ? { id: row.listView.toLowerCase() } : undefined,
		};
	});

	// Transform folders
	const folderData = folders.map((folder) => {
		const parentId = folder.parentId > 0 ? parentMap.get(folder.parentId) : null;

		return {
			allowedTemplates: [],
			defaultTemplate: null,
			id: formatGuid(folder.uniqueId),
			alias: (folder.text || 'folder').toLowerCase().replace(/\s+/g, ''),
			name: folder.text || 'Unnamed Folder',
			description: null,
			icon: 'icon-folder',
			allowedAsRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			hasChildren: hasChildrenMap.get(folder.id) || false,
			parent: parentId ? { id: parentId } : null,
			isFolder: true,
			properties: [],
			containers: [],
			allowedDocumentTypes: [],
			compositions: [],
			cleanup: {
				preventCleanup: false,
				keepAllVersionsNewerThanDays: null,
				keepLatestVersionPerDayForDays: null,
			},
			flags: [] as string[],
		};
	});

	// Combine folders and document types
	const allDocumentTypes = [...folderData, ...documentTypes];

	// Generate TypeScript content
	// Note: compositions.compositionType needs to be converted to enum values
	const content = `import type { UmbMockDocumentTypeModel } from '../../types/mock-data-set.types.js';
import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string composition type to enum
function mapCompositionType(type: string): CompositionTypeModel {
	switch (type) {
		case 'Composition': return CompositionTypeModel.COMPOSITION;
		case 'Inheritance': return CompositionTypeModel.INHERITANCE;
		default: return CompositionTypeModel.COMPOSITION;
	}
}

const rawData = ${JSON.stringify(allDocumentTypes, null, '\t')};

export const data: Array<UmbMockDocumentTypeModel> = rawData.map(dt => ({
	...dt,
	compositions: dt.compositions.map(c => ({
		...c,
		compositionType: mapCompositionType(c.compositionType),
	})),
}));
`;

	writeDataFile('document-type.data.ts', content);
	console.log(`Transformed ${documentTypes.length} document types and ${folders.length} folders`);
}
