import { UMB_VALIDATION_CONTEXT } from '../context/validation.context-token.js';
import type { UmbValidationMessage } from '../context/validation-messages.manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const CtrlSymbol = Symbol();
const ObserveSymbol = Symbol();

export class UmbObserveValidationStateController extends UmbControllerBase {
	constructor(
		host: UmbControllerHost,
		dataPath: string | undefined,
		callback: (messages: Array<UmbValidationMessage>) => void,
	) {
		super(host, CtrlSymbol);
		if (dataPath) {
			this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
				this.observe(context.messages.messagesOfPathAndDescendant(dataPath), callback, ObserveSymbol);
			});
		}
	}
}
