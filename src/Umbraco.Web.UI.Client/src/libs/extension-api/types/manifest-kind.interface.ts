export interface ManifestKind<ManifestTypes> {
	type: 'kind';
	alias: string;
	matchType: string;
	matchKind: string;
	/**
	 * Provide pre defined properties for the extension manifest.
	 * Define the `type`-property and other properties you like to preset for implementations of this kind.
	 * @example {
	 *	type: 'section',
	 * 	weight: 123,
	 * }
	 * @TJS-type object
	 */
	manifest: Partial<ManifestTypes>;
}
