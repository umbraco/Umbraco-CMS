import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbTest1PropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource
{
	constructor(host: UmbControllerHost) {
		super(host);
		debugger;
	}

	async execute(): Promise<void> {
		debugger;
	}
}

export { UmbTest1PropertyEditorDataSource as api };
