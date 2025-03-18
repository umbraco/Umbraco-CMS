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
			const message = 'data source identifier is required';
			console.error(message);
			throw new Error(message);
		}

		if (!args.forDataModel) {
			const message = 'forDataModel is missing';
			console.error(message);
			throw new Error(message);
		}

		if (!args.data) {
			const message = 'data is required';
			console.error(message);
			throw new Error(message);
		}

		const dataMapping = await this.#dataMappingResolver.resolve(args.forDataSource, args.forDataModel);

		if (!dataMapping && !args.fallback) {
			const message = 'Data mapping not found and no fallback provided.';
			console.error(message);
			throw new Error(message);
		}

		if (!dataMapping && args.fallback) {
			return args.fallback(args.data);
		}

		if (!dataMapping?.map) {
			const message = 'Data mapping does not have a map method.';
			console.error(message);
			throw new Error(message);
		}

		return dataMapping.map(args.data);
	}
}
