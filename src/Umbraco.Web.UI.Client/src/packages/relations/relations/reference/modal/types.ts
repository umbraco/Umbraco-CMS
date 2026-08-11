export interface UmbEntityReferencesModalData {
	unique: string;
	referenceRepositoryAlias: string;
	itemRepositoryAlias?: string;
	headline?: string;
	/** When set, also shows a read-only section listing elements this entity directly references that are not fully published. */
	includeReferencedElementsWithPendingChanges?: boolean;
}

export type UmbEntityReferencesModalValue = undefined;
