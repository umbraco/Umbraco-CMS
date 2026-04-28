/**
 * Represents a single redirect URL associated with a document.
 */
export interface UmbDocumentRedirectUrlModel {
	unique: string;
	originalUrl: string;
	destinationUrl: string;
	created: string;
	documentUnique: string;
	culture: string | null;
}

/**
 * Represents the redirect URL tracker status.
 */
export interface UmbDocumentRedirectStatusModel {
	enabled: boolean;
	userIsAdmin: boolean;
}

/**
 * Filter arguments for paginated redirect URL queries.
 */
export interface UmbDocumentRedirectFilterArgs {
	filter?: string;
	skip?: number;
	take?: number;
}
