export interface UmbDocumentUrlsModel {
	unique: string;
	urls: Array<UmbDocumentUrlModel>;
}

export interface UmbDocumentUrlModel {
	culture?: string | null;
	url?: string;
}
