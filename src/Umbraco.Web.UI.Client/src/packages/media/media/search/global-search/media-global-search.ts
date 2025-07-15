import type { UmbMediaSearchProvider } from '../media.search-provider.js';
import {
	UmbGlobalSearchBase,
	type UmbGlobalSearchApi,
	type UmbSearchRequestArgs,
} from '@umbraco-cms/backoffice/search';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';

export class UmbMediaGlobalSearch extends UmbGlobalSearchBase<UmbMediaSearchProvider> implements UmbGlobalSearchApi {
	override async search(args: UmbSearchRequestArgs) {
		await this._init;

		if (!this._searchProvider) {
			throw new Error('Search provider is not available');
		}

		// TODO: change this to consume so we don't emit context events for every search change [MR]
		const variantContext = await this.getContext(UMB_VARIANT_CONTEXT);
		const displayCulture = await variantContext?.getDisplayCulture();

		return await this._searchProvider.search({ culture: displayCulture, query: args.query });
	}
}

export { UmbMediaGlobalSearch as api };
