import { UMB_PICKER_CONTEXT } from './picker.context.token.js';
import { UmbPickerSearchManager } from './search/manager/picker-search.manager.js';
import type { UmbPickerContextConfig } from './types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbPickerContext<
	ConfigType extends UmbPickerContextConfig = UmbPickerContextConfig,
> extends UmbContextBase<UmbPickerContext> {
	public readonly selection = new UmbSelectionManager(this);
	public readonly search = new UmbPickerSearchManager(this);

	#config = new UmbObjectState<ConfigType | undefined>(undefined);
	public readonly config = this.#config.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_CONTEXT);
	}

	/**
	 * Set the config for the picker
	 * @param {ConfigType} config
	 * @memberof UmbPickerContext
	 */
	setConfig(config: ConfigType | undefined) {
		const searchProviderAlias = config?.search?.providerAlias;
		if (searchProviderAlias) {
			this.search.updateConfig({ providerAlias: searchProviderAlias });
			this.search.setSearchable(true);
		} else {
			this.search.setSearchable(false);
		}
	}

	/**
	 * Get the data for the picker
	 * @returns {ConfigType | undefined}
	 * @memberof UmbPickerContext
	 */
	getConfig(): ConfigType | undefined {
		return this.#config.getValue();
	}
}
