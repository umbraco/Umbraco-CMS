import { UmbDataMappingResolver } from './mapping/data-mapping-resolver.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbDataMapperMapArgs<fromModelType = unknown, toModelType = unknown> {
	identifier: string;
	data: fromModelType;
	fallback?: (data: fromModelType) => Promise<toModelType>;
}

export class UmbDataMapper<fromModelType = unknown, toModelType = unknown> extends UmbControllerBase {
	#dataMappingResolver = new UmbDataMappingResolver(this);

	async map(args: UmbDataMapperMapArgs<fromModelType, toModelType>) {
		if (!args.identifier) {
			throw new Error('identifier is required');
		}

		if (!args.data) {
			throw new Error('data is required');
		}

		const dataMapping = await this.#dataMappingResolver.resolve(args.identifier);

		if (!dataMapping && !args.fallback) {
			throw new Error('Data Mapper not found and no fallback provided.');
		}

		if (!dataMapping && args.fallback) {
			return args.fallback(args.data);
		}

		if (!dataMapping?.map) {
			throw new Error('Data Mapper does not have a map method.');
		}

		return dataMapping.map(args.data);
	}
}
