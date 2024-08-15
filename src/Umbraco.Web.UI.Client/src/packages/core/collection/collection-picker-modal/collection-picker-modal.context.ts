import { UmbPickerModalContext } from '@umbraco-cms/backoffice/picker-modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCollectionItemPickerModalContext extends UmbPickerModalContext {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}

export { UmbCollectionItemPickerModalContext as api };
