import { UmbOEmbedServerDataSource } from './oembed.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbOEmbedRepository extends UmbControllerBase implements UmbApi {
	#dataSource = new UmbOEmbedServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	async requestOEmbed({ url, maxWidth, maxHeight }: { url?: string; maxWidth?: number; maxHeight?: number }) {
		const { data, error } = await this.#dataSource.getOEmbedQuery({ url, maxWidth, maxHeight });
		if (!error) {
			return { data };
		}
		return { error };
	}
}

export { UmbOEmbedRepository as api };
