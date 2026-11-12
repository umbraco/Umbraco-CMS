import { UMB_FILTER_CONTEXT } from '../filter.context-token.js';
import type { ManifestFilter } from '../filter.extension.js';
import type { UmbSelectFilterOptionModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

/**
 * Base for the APIs of select filters: holds the options, the selected values and their items.
 * Subclasses decide where the options come from and how the selected values are resolved into items.
 */
export abstract class UmbSelectFilterApiBase<
	OptionType extends UmbSelectFilterOptionModel = UmbSelectFilterOptionModel,
> extends UmbControllerBase {
	#options = new UmbArrayState<OptionType>([], (x) => x.unique);
	public readonly options = this.#options.asObservable();

	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#valueItems = new UmbArrayState<UmbItemModel>([], (x) => x.unique);
	public readonly valueItems = this.#valueItems.asObservable();

	public readonly pagination = new UmbPaginationManager();

	#filterContext?: typeof UMB_FILTER_CONTEXT.TYPE;

	#manifest?: ManifestFilter;
	public get manifest(): ManifestFilter | undefined {
		return this.#manifest;
	}
	public set manifest(manifest: ManifestFilter | undefined) {
		this.#manifest = manifest;
		this._onManifest(manifest);
	}

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_FILTER_CONTEXT, (context) => {
			this.#filterContext = context;
			this.observe(
				context?.values,
				(entries) => {
					const values = entries?.map((entry) => entry.value as string) ?? [];
					this.#value.setValue(values);
					this._requestValueItems(values);
				},
				'umbFilterValuesObserver',
			);
		});
	}

	/**
	 * Loads the first page of options, for filters whose options are requested on demand.
	 */
	public loadOptions(): void {}

	/**
	 * Loads the next page of options, for filters whose options are requested on demand.
	 */
	public loadMoreOptions(): void {}

	/**
	 * Sets the selected values of the filter. An empty selection clears the filter.
	 * @param {Array<string>} values - The selected values.
	 */
	public setValue(values: Array<string>): void {
		const filtered = values.filter((value) => value !== '');

		if (filtered.length === 0) {
			this.#filterContext?.clearAllValues();
		} else {
			this.#filterContext?.setValues(filtered.map((value) => ({ unique: value, value })));
		}
	}

	protected abstract _onManifest(manifest: ManifestFilter | undefined): void;

	protected abstract _requestValueItems(uniques: Array<string>): void;

	protected _getOptions(): Array<OptionType> {
		return this.#options.getValue();
	}

	protected _setOptions(options: Array<OptionType>): void {
		this.#options.setValue(options);
	}

	protected _setValueItems(items: Array<UmbItemModel>): void {
		this.#valueItems.setValue(items);
	}
}
