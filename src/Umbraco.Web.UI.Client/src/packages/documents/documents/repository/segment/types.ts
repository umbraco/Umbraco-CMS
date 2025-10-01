export interface UmbDocumentSegmentFilterModel {
	skip?: number;
	take?: number;
}

export interface UmbDocumentSegmentModel {
	alias: string;
	name: string;
	cultures?: Array<string> | null;
}

export interface UmbDocumentSegmentResponseModel {
	items: Array<UmbDocumentSegmentModel>;
	total: number;
}
