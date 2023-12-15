import { UmbDataTypeDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ACTION_EVENT_CONTEXT, type UmbActionEventContext } from '@umbraco-cms/backoffice/action';

/**
 * @export
 * @class UmbDataTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Data Type Details
 */
export class UmbDataTypeDetailStore extends UmbDetailStoreBase<UmbDataTypeDetailModel> {
	#actionEventContext?: UmbActionEventContext;

	/**
	 * Creates an instance of UmbDataTypeDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DATA_TYPE_DETAIL_STORE_CONTEXT.toString());

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;
			this.#listen();
		});
	}

	#listen() {
		this.#actionEventContext!.addEventListener('save-success', (event) => {
			console.log('event', event);
			debugger;
		});

		this.#actionEventContext!.addEventListener('save-error', (event) => {
			console.log('event', event);
			debugger;
		});
	}

	withPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		// TODO: Use a model for the data-type tree items: ^^Most likely it should be parsed to the UmbEntityTreeStore as a generic type.
		return this._data.asObservablePart((items) =>
			items.filter((item) => item.propertyEditorUiAlias === propertyEditorUiAlias),
		);
	}
}

export const UMB_DATA_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDataTypeDetailStore>('UmbDataTypeDetailStore');
