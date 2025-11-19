import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../global-contexts/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbDocumentAllowEditInvariantFromNonDefaultControllerBase extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT, async (context) => {
			const config = await context?.getDocumentConfiguration();
			const allowEditInvariantFromNonDefault = config?.allowEditInvariantFromNonDefault ?? true;

			if (allowEditInvariantFromNonDefault === false) {
				this._preventEditInvariantFromNonDefault();
			}
		});
	}

	protected abstract _preventEditInvariantFromNonDefault(): Promise<void>;
}
