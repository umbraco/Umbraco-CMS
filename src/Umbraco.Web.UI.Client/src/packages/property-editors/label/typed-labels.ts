import type { ManifestPropertyEditorSchema, ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

/**
 * The label editors that hold something other than a string.
 *
 * There is one label editor per type of value a label can hold, so that the type a label property yields - and the
 * column its value is stored in - follow from the editor rather than from configuration. They all share the one
 * element, as the type only decides what is stored, not how a read-only value is displayed.
 */
const typedLabels = [
	{
		schemaAlias: 'Umbraco.Label.Text',
		uiAlias: 'Umb.PropertyEditorUi.Label.Text',
		label: 'Label (long string)',
		keywords: ['text', 'long', 'string'],
	},
	{
		schemaAlias: 'Umbraco.Label.Integer',
		uiAlias: 'Umb.PropertyEditorUi.Label.Integer',
		label: 'Label (integer)',
		keywords: ['integer', 'number', 'int'],
	},
	{
		schemaAlias: 'Umbraco.Label.BigInt',
		uiAlias: 'Umb.PropertyEditorUi.Label.BigInt',
		label: 'Label (big integer)',
		keywords: ['integer', 'number', 'bigint', 'long'],
	},
	{
		schemaAlias: 'Umbraco.Label.Decimal',
		uiAlias: 'Umb.PropertyEditorUi.Label.Decimal',
		label: 'Label (decimal)',
		keywords: ['decimal', 'number', 'fraction'],
	},
	{
		schemaAlias: 'Umbraco.Label.DateTime',
		uiAlias: 'Umb.PropertyEditorUi.Label.DateTime',
		label: 'Label (date and time)',
		keywords: ['date', 'time', 'datetime'],
	},
	{
		schemaAlias: 'Umbraco.Label.Time',
		uiAlias: 'Umb.PropertyEditorUi.Label.Time',
		label: 'Label (time)',
		keywords: ['time'],
	},
] as const;

/**
 * The label template is what turns a stored value into something worth reading, so every label editor takes one -
 * the built-in "bytes" and "pixels" labels are the integer editors with a template.
 */
const labelTemplateSetting = {
	alias: 'labelTemplate',
	label: 'Label template',
	description: 'Enter a template for the label.',
	propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
};

const schemaManifests: Array<ManifestPropertyEditorSchema> = typedLabels.map((typedLabel) => ({
	type: 'propertyEditorSchema',
	name: typedLabel.label,
	alias: typedLabel.schemaAlias,
	meta: {
		defaultPropertyEditorUiAlias: typedLabel.uiAlias,
	},
}));

const uiManifests: Array<ManifestPropertyEditorUi> = typedLabels.map((typedLabel) => ({
	type: 'propertyEditorUi',
	alias: typedLabel.uiAlias,
	name: `${typedLabel.label} Property Editor UI`,
	element: () => import('./property-editor-ui-label.element.js'),
	meta: {
		label: typedLabel.label,
		icon: 'icon-readonly',
		group: '#propertyEditorUIGroups_common',
		keywords: ['readonly', 'display', 'static', 'computed', 'info', ...typedLabel.keywords],
		propertyEditorSchemaAlias: typedLabel.schemaAlias,
		supportsReadOnly: true,
		settings: {
			properties: [labelTemplateSetting],
		},
	},
}));

export const manifests: Array<UmbExtensionManifest> = [...schemaManifests, ...uiManifests];
