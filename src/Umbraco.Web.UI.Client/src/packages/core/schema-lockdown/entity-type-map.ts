const FOLDER_SUFFIX = '-folder';

/**
 * Translates a backoffice entity type to the entity type the server reports it as.
 * Entity types named the same on both sides are returned unchanged.
 * @param {string} entityType The backoffice entity type, e.g. 'document-type'.
 * @returns {string} The server's entity type.
 */
export function toSchemaEntityType(entityType: string): string {
	// The server governs folders as part of the entity type they hold.
	if (entityType.endsWith(FOLDER_SUFFIX)) {
		return toSchemaEntityType(entityType.slice(0, -FOLDER_SUFFIX.length));
	}

	switch (entityType) {
		case 'dictionary':
			return 'dictionary-item';
		default:
			return entityType;
	}
}
