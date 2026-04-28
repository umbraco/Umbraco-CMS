export interface UmbDocumentRedirectUrlModel {
	unique: string;
	originalUrl: string;
	destinationUrl: string;
	created: string;
	documentUnique: string;
	culture: string | null;
}

export interface UmbDocumentRedirectStatusModel {
	enabled: boolean;
	userIsAdmin: boolean;
}

export interface UmbDocumentRedirectFilterArgs {
	filter?: string;
	skip?: number;
	take?: number;
}
