import type {
	ManifestGlobalSearch,
	ManifestSearchProvider,
	UmbGlobalSearchApi,
	UmbSearchProvider,
	UmbSearchRequestArgs,
} from '../types.js';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export abstract class UmbGlobalSearchBase<SearchProviderType extends UmbSearchProvider = UmbSearchProvider>
	extends UmbControllerBase
	implements UmbGlobalSearchApi
{
	protected _manifest?: ManifestGlobalSearch;
	protected _searchProvider?: SearchProviderType;
	#initResolver?: () => void;
	#initialized = false;

	protected _init = new Promise<void>((resolve) => {
		if (this.#initialized) {
			resolve();
		} else {
			this.#initResolver = resolve;
		}
	});

	#checkIfInitialized() {
		if (this._searchProvider) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	public set manifest(manifest: ManifestGlobalSearch | undefined) {
		if (this._manifest === manifest) return;
		this._manifest = manifest;
		this.#observeSearchProvider(this._manifest?.meta.searchProviderAlias);
	}
	public get manifest() {
		return this._manifest;
	}

	#observeSearchProvider(alias?: string) {
		if (!alias) throw new Error('Search provider alias is required');

		new UmbExtensionApiInitializer<ManifestSearchProvider>(
			this,
			umbExtensionsRegistry,
			alias,
			[this],
			(permitted, ctrl) => {
				this._searchProvider = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}

	async search(args: UmbSearchRequestArgs) {
		await this._init;

		if (!this._searchProvider) {
			throw new Error('Search provider is not available');
		}

		return await this._searchProvider.search({ query: args.query });
	}
}
