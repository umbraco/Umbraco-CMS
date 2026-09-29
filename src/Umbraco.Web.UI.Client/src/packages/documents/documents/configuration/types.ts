export interface UmbDocumentConfigurationModel {
	disableDeleteWhenReferenced: boolean;
	disableUnpublishWhenReferenced: boolean;
	allowEditInvariantFromNonDefault: boolean;
	/**
	 * @deprecated Will be removed in Umbraco 19. [NL]
	 */
	allowNonExistingSegmentsCreation: boolean;
}
