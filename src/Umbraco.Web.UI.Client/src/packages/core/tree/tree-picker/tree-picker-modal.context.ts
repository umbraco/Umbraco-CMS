import { UmbPickerModalContext } from '@umbraco-cms/backoffice/picker';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTreePickerModalContext extends UmbPickerModalContext {
	constructor(host: UmbControllerHost) {
		super(host);
		debugger;
	}
}

export { UmbTreePickerModalContext as api };
