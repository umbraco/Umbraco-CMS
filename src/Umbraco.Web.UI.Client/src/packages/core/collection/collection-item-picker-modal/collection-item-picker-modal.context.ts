import { UmbPickerContext } from '@umbraco-cms/backoffice/picker';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCollectionItemPickerContext extends UmbPickerContext {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}

export { UmbCollectionItemPickerContext as api };
