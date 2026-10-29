// Server entity types don't always match the backoffice ones ('dictionary-item' vs 'dictionary'), so every pair is
// listed. The keys are literals because core can't import the packages' entity type constants.
const ENTITY_TYPE_TO_SCHEMA_ENTITY_TYPE: Record<string, string> = {
	'document-type': 'document-type',
	'media-type': 'media-type',
	'member-type': 'member-type',
	'data-type': 'data-type',
	script: 'script',
	stylesheet: 'stylesheet',
	template: 'template',
	'partial-view': 'partial-view',
	dictionary: 'dictionary-item',
	language: 'language',
	webhook: 'webhook',
	'member-group': 'member-group',
	// The server governs folders as part of the entity type they hold.
	'document-type-folder': 'document-type',
	'media-type-folder': 'media-type',
	'member-type-folder': 'member-type',
	'data-type-folder': 'data-type',
	'script-folder': 'script',
	'stylesheet-folder': 'stylesheet',
	'partial-view-folder': 'partial-view',
};

/**
 * Translates a backoffice entity-type string to the entity type the server reports it as.
 * @param {string} entityType The backoffice entity-type string, e.g. 'document-type'.
 * @returns {string | undefined} The server's entity type, or undefined if schema lockdown doesn't cover it.
 */
export function toSchemaEntityType(entityType: string): string | undefined {
	return ENTITY_TYPE_TO_SCHEMA_ENTITY_TYPE[entityType];
}
