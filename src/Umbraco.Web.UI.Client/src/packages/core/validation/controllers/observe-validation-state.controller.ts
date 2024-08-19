import { UMB_VALIDATION_CONTEXT } from '../context/validation.context-token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const ObserveSymbol = Symbol();

export class UmbObserveValidationStateController extends UmbControllerBase {
	constructor(
		host: UmbControllerHost,
		dataPath: string | undefined,
		callback: (messages: boolean) => void,
		controllerAlias?: string,
	) {
		super(host, controllerAlias ?? 'observeValidationState_' + dataPath);
		if (dataPath) {
			this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
				this.observe(context.messages.hasMessagesOfPathAndDescendant(dataPath), callback, ObserveSymbol);
			});
		}
	}
}
