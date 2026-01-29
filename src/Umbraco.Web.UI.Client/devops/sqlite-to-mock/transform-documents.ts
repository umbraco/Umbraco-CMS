/**
 * Transform umbracoDocument records into mock data format.
 */
import { prepare, ObjectTypes, formatGuid, writeDataFile, type UmbracoNode } from './db.js';

interface DocumentRow {
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
}

interface DocumentPublishState {
	nodeId: number;
	published: number;
	edited: number;
}

interface ContentVersion {
	id: number;
	nodeId: number;
	versionDate: string;
	userId: number | null;
	current: number;
	text: string | null;
	preventCleanup: number;
}

interface DocumentCultureVariation {
	nodeId: number;
	languageId: number;
	culture: string;
	name: string | null;
	edited: number;
	available: number;
	published: number;
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

interface LanguageInfo {
	id: number;
	isoCode: string;
}

export function transformDocuments(): void {
	// Query documents with their content type information
	const query = prepare(`
		SELECT
			n.id as nodeId, n.uniqueId, n.parentId, n.level, n.path, n.sortOrder,
			n.trashed, n.text, n.createDate,
			c.contentTypeId as contentTypeNodeId,
			ct.alias as contentTypeAlias,
			ct.icon as contentTypeIcon
		FROM umbracoNode n
		JOIN umbracoContent c ON n.id = c.nodeId
		JOIN cmsContentType ct ON c.contentTypeId = ct.nodeId
		WHERE n.nodeObjectType = ?
		ORDER BY n.level, n.sortOrder
	`);

	const rows = query.all(ObjectTypes.Document) as DocumentRow[];

	// Get document publish state
	const publishStateQuery = prepare(`
		SELECT nodeId, published, edited
		FROM umbracoDocument
	`);

	const publishStates = publishStateQuery.all() as DocumentPublishState[];
	const publishStateMap = new Map<number, DocumentPublishState>();
	for (const ps of publishStates) {
		publishStateMap.set(ps.nodeId, ps);
	}

	// Get current content versions
	const versionQuery = prepare(`
		SELECT id, nodeId, versionDate, userId, current, text, preventCleanup
		FROM umbracoContentVersion
		WHERE current = 1
	`);

	const versions = versionQuery.all() as ContentVersion[];
	const versionMap = new Map<number, ContentVersion>();
	for (const v of versions) {
		versionMap.set(v.nodeId, v);
	}

	// Get document culture variations
	const cultureQuery = prepare(`
		SELECT
			dcv.nodeId, dcv.languageId, l.languageISOCode as culture,
			dcv.name, dcv.edited, dcv.available, dcv.published
		FROM umbracoDocumentCultureVariation dcv
		JOIN umbracoLanguage l ON dcv.languageId = l.id
	`);

	const cultures = cultureQuery.all() as DocumentCultureVariation[];
	const culturesByNode = new Map<number, DocumentCultureVariation[]>();
	for (const c of cultures) {
		const list = culturesByNode.get(c.nodeId) || [];
		list.push(c);
		culturesByNode.set(c.nodeId, list);
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

	// Get languages
	const languageQuery = prepare(`
		SELECT id, languageISOCode as isoCode
		FROM umbracoLanguage
	`);

	const languages = languageQuery.all() as LanguageInfo[];
	const languageMap = new Map<number, string>();
	for (const lang of languages) {
		languageMap.set(lang.id, lang.isoCode);
	}

	// Get content type nodes for ID lookup
	const contentTypeNodeQuery = prepare(`
		SELECT id, uniqueId
		FROM umbracoNode
		WHERE nodeObjectType = ?
	`);

	const contentTypeNodes = contentTypeNodeQuery.all(ObjectTypes.DocumentType) as { id: number; uniqueId: string }[];
	const contentTypeIdMap = new Map<number, string>();
	for (const ct of contentTypeNodes) {
		contentTypeIdMap.set(ct.id, formatGuid(ct.uniqueId));
	}

	// Build parent lookup map (nodeId -> uniqueId)
	const nodeIdMap = new Map<number, string>();
	for (const row of rows) {
		nodeIdMap.set(row.nodeId, formatGuid(row.uniqueId));
	}

	// Check if documents have children
	const hasChildrenMap = new Map<number, boolean>();
	for (const row of rows) {
		if (row.parentId > 0) {
			hasChildrenMap.set(row.parentId, true);
		}
	}

	// Transform documents
	const documents = rows.map((row) => {
		const publishState = publishStateMap.get(row.nodeId);
		const version = versionMap.get(row.nodeId);
		const nodeCultures = culturesByNode.get(row.nodeId) || [];
		const parentId = row.parentId > 0 ? nodeIdMap.get(row.parentId) : null;

		// Build ancestors array from path
		const pathParts = row.path.split(',').filter((p) => p !== '-1' && p !== String(row.nodeId));
		const ancestors = pathParts.map((id) => ({ id: nodeIdMap.get(Number(id)) || id }));

		// Get property values for this document's version
		const versionPropertyData = version ? propertyDataByVersion.get(version.id) || [] : [];

		// Transform property values
		const values = versionPropertyData.map((pd) => {
			const propType = propertyTypeMap.get(pd.propertyTypeId);
			if (!propType) return null;

			// Determine the value based on what's populated
			let value: unknown = null;
			if (pd.textValue !== null) {
				// Try to parse as JSON first
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
				culture: pd.languageId ? languageMap.get(pd.languageId) || null : null,
				segment: pd.segment,
				value,
			};
		}).filter((v): v is NonNullable<typeof v> => v !== null);

		// Build variants
		const variants = nodeCultures.length > 0
			? nodeCultures.map((c) => ({
					state: c.published === 1 ? 'Published' : 'Draft',
					publishDate: version?.versionDate || row.createDate,
					culture: c.culture,
					segment: null,
					name: c.name || row.text || 'Unnamed',
					createDate: row.createDate,
					updateDate: version?.versionDate || row.createDate,
					id: formatGuid(row.uniqueId),
					flags: [] as string[],
				}))
			: [
					{
						state: publishState?.published ? 'Published' : 'Draft',
						publishDate: version?.versionDate || row.createDate,
						culture: null,
						segment: null,
						name: row.text || 'Unnamed',
						createDate: row.createDate,
						updateDate: version?.versionDate || row.createDate,
						id: formatGuid(row.uniqueId),
						flags: [] as string[],
					},
				];

		return {
			ancestors,
			template: null,
			id: formatGuid(row.uniqueId),
			createDate: row.createDate,
			parent: parentId ? { id: parentId } : null,
			documentType: {
				id: contentTypeIdMap.get(row.contentTypeNodeId) || `unknown-${row.contentTypeNodeId}`,
				icon: row.contentTypeIcon || 'icon-document',
			},
			hasChildren: hasChildrenMap.get(row.nodeId) || false,
			noAccess: false,
			isProtected: false,
			isTrashed: row.trashed === 1,
			variants,
			values,
			flags: [] as string[],
		};
	});

	// Filter out trashed documents
	const nonTrashedDocuments = documents.filter((d) => !d.isTrashed);

	// Generate TypeScript content
	// Note: variants.state needs to be converted to enum values
	const content = `import type { UmbMockDocumentModel } from '../../types/mock-data-set.types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string state to enum
function mapState(state: string): DocumentVariantStateModel {
	switch (state) {
		case 'Published': return DocumentVariantStateModel.PUBLISHED;
		case 'Draft': return DocumentVariantStateModel.DRAFT;
		case 'NotCreated': return DocumentVariantStateModel.NOT_CREATED;
		case 'PublishedPendingChanges': return DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES;
		default: return DocumentVariantStateModel.DRAFT;
	}
}

const rawData = ${JSON.stringify(nonTrashedDocuments, null, '\t')};

export const data: Array<UmbMockDocumentModel> = rawData.map(doc => ({
	...doc,
	variants: doc.variants.map(v => ({
		...v,
		state: mapState(v.state),
	})),
}));
`;

	writeDataFile('document.data.ts', content);
	console.log(`Transformed ${nonTrashedDocuments.length} documents (${documents.length - nonTrashedDocuments.length} trashed excluded)`);
}

// Run if called directly
transformDocuments();
