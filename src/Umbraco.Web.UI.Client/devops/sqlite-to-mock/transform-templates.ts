/**
 * Transform cmsTemplate records into mock data format.
 */
import { prepare, ObjectTypes, formatGuid, writeDataFile, type UmbracoNode } from './db.js';

interface TemplateRow extends UmbracoNode {
	alias: string | null;
}

export function transformTemplates(): void {
	// Query templates with their node information
	// Note: cmsTemplate only has pk, nodeId, alias - design/content is stored on disk
	const query = prepare(`
		SELECT
			n.id, n.uniqueId, n.parentId, n.level, n.path, n.sortOrder,
			n.trashed, n.nodeUser, n.text, n.nodeObjectType, n.createDate,
			t.alias
		FROM umbracoNode n
		JOIN cmsTemplate t ON n.id = t.nodeId
		WHERE n.nodeObjectType = ?
		  AND n.trashed = 0
		ORDER BY n.sortOrder
	`);

	const rows = query.all(ObjectTypes.Template) as TemplateRow[];

	// Build parent lookup map (nodeId -> uniqueId)
	const parentMap = new Map<number, string>();
	for (const row of rows) {
		parentMap.set(row.id, formatGuid(row.uniqueId));
	}

	// Check if templates have children
	const hasChildrenMap = new Map<number, boolean>();
	for (const row of rows) {
		if (row.parentId > 0) {
			hasChildrenMap.set(row.parentId, true);
		}
	}

	// Transform templates
	const templates = rows.map((row) => {
		const parentId = row.parentId > 0 ? parentMap.get(row.parentId) : null;

		// Generate placeholder template content
		const templateContent = `@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@{
	Layout = ${parentId ? `"${row.alias}"` : 'null'};
}`;

		return {
			id: formatGuid(row.uniqueId),
			parent: parentId ? { id: parentId } : null,
			name: row.text || 'Unnamed',
			hasChildren: hasChildrenMap.get(row.id) || false,
			alias: row.alias || 'unnamed',
			flags: [] as string[],
			content: templateContent,
			masterTemplate: parentId ? { id: parentId } : undefined,
		};
	});

	// Generate TypeScript content
	const content = `import type { UmbMockTemplateModel } from '../../types/mock-data-set.types.js';
import type {
	TemplateQuerySettingsResponseModel,
	TemplateQueryResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TemplateQueryPropertyTypeModel, OperatorModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<UmbMockTemplateModel> = ${JSON.stringify(templates, null, '\t')};

export const templateQueryResult: TemplateQueryResultResponseModel = {
	queryExpression: '',
	sampleResults: [],
	resultCount: 0,
	executionTime: 0,
};

export const templateQuerySettings: TemplateQuerySettingsResponseModel = {
	documentTypeAliases: [],
	properties: [
		{
			alias: 'Id',
			type: TemplateQueryPropertyTypeModel.INTEGER,
		},
		{
			alias: 'Name',
			type: TemplateQueryPropertyTypeModel.STRING,
		},
		{
			alias: 'CreateDate',
			type: TemplateQueryPropertyTypeModel.DATE_TIME,
		},
		{
			alias: 'UpdateDate',
			type: TemplateQueryPropertyTypeModel.DATE_TIME,
		},
	],
	operators: [
		{
			operator: OperatorModel.EQUALS,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.STRING],
		},
		{
			operator: OperatorModel.NOT_EQUALS,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.STRING],
		},
		{
			operator: OperatorModel.LESS_THAN,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.DATE_TIME],
		},
		{
			operator: OperatorModel.GREATER_THAN,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.DATE_TIME],
		},
		{
			operator: OperatorModel.CONTAINS,
			applicableTypes: [TemplateQueryPropertyTypeModel.STRING],
		},
	],
};
`;

	writeDataFile('template.data.ts', content);
	console.log(`Transformed ${templates.length} templates`);
}

// Run if called directly
transformTemplates();
