import { UmbDataMapperResolver } from './data-mapper-resolver.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbDataMapperMapArgs<fromModelType = unknown, toModelType = unknown> {
	identifier: string;
	data: fromModelType;
	fallback?: (data: fromModelType) => Promise<toModelType>;
}

export class UmbDataMapper<fromModelType = unknown, toModelType = unknown> extends UmbControllerBase {
	#dataMapperResolver = new UmbDataMapperResolver(this);

	async map(args: UmbDataMapperMapArgs<fromModelType, toModelType>) {
		if (!args.identifier) {
			throw new Error('identifier is required');
		}

		if (!args.data) {
			throw new Error('data is required');
		}

		const mapper = await this.#dataMapperResolver.resolve(args.identifier);

		if (!mapper && !args.fallback) {
			throw new Error('Data Mapper not found and no fallback provided.');
		}

		if (!mapper && args.fallback) {
			return args.fallback(args.data);
		}

		if (!mapper?.map) {
			throw new Error('Data Mapper does not have a map method.');
		}

		return mapper.map(args.data);
	}
}
