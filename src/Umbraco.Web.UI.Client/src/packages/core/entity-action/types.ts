export interface UmbEntityActionArgs<MetaArgsType> {
	entityType: string;
	unique: string | null;
	meta: MetaArgsType;
}
