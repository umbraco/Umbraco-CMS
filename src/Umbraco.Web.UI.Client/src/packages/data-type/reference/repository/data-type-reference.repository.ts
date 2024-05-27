import { UmbDataTypeReferenceServerDataSource } from './data-type-reference.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { DataTypeReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export type UmbDataTypeReferenceModel = {
	unique: string;
	entityType: string | null;
	name: string | null;
	icon: string | null;
	properties: Array<{ name: string; alias: string }>;
};

export class UmbDataTypeReferenceRepository extends UmbControllerBase {
	#referenceSource: UmbDataTypeReferenceServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#referenceSource = new UmbDataTypeReferenceServerDataSource(this);
	}

	async requestReferencedBy(unique: string) {
		if (!unique) throw new Error(`unique is required`);

		const { data } = await this.#referenceSource.getReferencedBy(unique);
		if (!data) return;

		return data.map(mapper);
	}
}

const mapper = (item: DataTypeReferenceResponseModel): UmbDataTypeReferenceModel => {
	return {
		unique: item.contentType.id,
		entityType: item.contentType.type,
		name: item.contentType.name,
		icon: item.contentType.icon,
		properties: item.properties,
	};
};

export default UmbDataTypeReferenceRepository;
