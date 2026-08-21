// The server keys the matrix on its own entity-type vocabulary; backoffice code keys on the entity-type
// strings each package exports. Nine of the ten currently coincide and dictionary does not - the server
// says 'dictionary-item' where the backoffice says 'dictionary' - so the correspondence is coincidence
// rather than contract, and a future type could diverge the same way. Every pair is therefore listed
// explicitly: nothing here may fall back to assuming the two sides spell an entity type the same way.
//
// The keys below are inlined literals rather than the packages' exported entity-type constants
// to avoid a core→package dependency the module-dependency guard forbids. Don't re-import them.
const ENTITY_TYPE_TO_SCHEMA_ENTITY_TYPE: Record<string, string> = {
	'document-type': 'document-type',
	'media-type': 'media-type',
	'member-type': 'member-type',
	'data-type': 'data-type',
	script: 'script',
	stylesheet: 'stylesheet',
	dictionary: 'dictionary-item',
	language: 'language',
	webhook: 'webhook',
	'document-blueprint': 'document-blueprint',
};

/**
 * Translates a backoffice entity-type string to the entity type the server reports it as.
 * @param {string} entityType The backoffice entity-type string, e.g. 'document-type'.
 * @returns {string | undefined} The server's entity type, or undefined if the entity type is not part of the schema lockdown matrix.
 */
export function toSchemaEntityType(entityType: string): string | undefined {
	return ENTITY_TYPE_TO_SCHEMA_ENTITY_TYPE[entityType];
}
