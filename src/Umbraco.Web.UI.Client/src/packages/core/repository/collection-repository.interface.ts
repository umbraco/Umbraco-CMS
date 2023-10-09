export interface UmbCollectionRepository {
	requestCollection(filter?: any): Promise<any>;
}
