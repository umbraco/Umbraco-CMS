export interface UmbStructureItemModelBase {
	unique: string | null;
	entityType: string;
}

export interface UmbStructureItemModel extends UmbStructureItemModelBase {
	name: string;
}

export interface UmbVariantStructureItemModel extends UmbStructureItemModelBase {
	variants: Array<{ name: string; culture: string | null; segment: string | null }>;
}
