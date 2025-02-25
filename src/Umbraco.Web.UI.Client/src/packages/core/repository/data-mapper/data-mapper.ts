import { UmbDataSourceDataMappingResolver } from './mapping/data-mapping-resolver.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbDataSourceDataMapperMapArgs<fromModelType = unknown, toModelType = unknown> {
	forDataModel: string;
	forDataSource: string;
	data: fromModelType;
	fallback?: (data: fromModelType) => Promise<toModelType>;
}

export class UmbDataSourceDataMapper<fromModelType = unknown, toModelType = unknown> extends UmbControllerBase {
	#dataMappingResolver = new UmbDataSourceDataMappingResolver(this);

	async map(args: UmbDataSourceDataMapperMapArgs<fromModelType, toModelType>) {
		if (!args.forDataSource) {
			throw new Error('data source identifier is required');
		}

		if (!args.forDataModel) {
			throw new Error('data identifier is required');
		}

		if (!args.data) {
			throw new Error('data is required');
		}

		const dataMapping = await this.#dataMappingResolver.resolve(args.forDataSource, args.forDataModel);

		if (!dataMapping && !args.fallback) {
			throw new Error('Data mapping not found and no fallback provided.');
		}

		if (!dataMapping && args.fallback) {
			return args.fallback(args.data);
		}

		if (!dataMapping?.map) {
			throw new Error('Data mapping does not have a map method.');
		}

		return dataMapping.map(args.data);
	}
}
