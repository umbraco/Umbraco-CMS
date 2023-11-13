
export interface ManifestKind<ManifestTypes> {
	type: 'kind';
	alias: string;
	matchType: string;
	matchKind: string;
	manifest: Partial<ManifestTypes>;
}