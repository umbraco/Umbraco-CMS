import { UMB_PICKER_MODAL_CONTEXT } from './picker-modal.context.token.js';
import { UmbPickerModalSearchManager } from './search/manager/picker-modal-search.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UmbPickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbPickerModalContext<
	ModalDataType extends UmbPickerModalData<any> = UmbPickerModalData<any>,
> extends UmbContextBase<UmbPickerModalContext> {
	public readonly selection = new UmbSelectionManager(this);
	public readonly search = new UmbPickerModalSearchManager(this);

	#data = new UmbObjectState<ModalDataType | undefined>(undefined);
	public readonly data = this.#data.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_MODAL_CONTEXT);
	}

	/**
	 * Set the data for the picker modal
	 * @param {ModalDataType} data
	 * @memberof UmbPickerModalContext
	 */
	setData(data: ModalDataType | undefined) {
		const searchProviderAlias = data?.search?.providerAlias;
		if (searchProviderAlias) {
			this.search.setConfig({ providerAlias: searchProviderAlias });
			this.search.setSearchable(true);
		} else {
			this.search.setConfig({ providerAlias: undefined });
			this.search.setSearchable(false);
		}
	}

	/**
	 * Get the data for the picker modal
	 * @returns {ModalDataType | undefined}
	 * @memberof UmbPickerModalContext
	 */
	getData(): ModalDataType | undefined {
		return this.#data.getValue();
	}
}
