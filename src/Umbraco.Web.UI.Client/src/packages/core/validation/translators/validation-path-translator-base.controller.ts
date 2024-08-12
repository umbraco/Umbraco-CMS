import type { UmbValidationMessageTranslator } from './validation-message-path-translator.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_VALIDATION_CONTEXT } from '../index.js';

export abstract class UmbValidationPathTranslatorBase
	extends UmbControllerBase
	implements UmbValidationMessageTranslator
{
	//
	protected _context?: typeof UMB_VALIDATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			this._context?.removeTranslator(this);
			this._context = context;
			context.addTranslator(this);
		});
	}

	override hostDisconnected(): void {
		this._context?.removeTranslator(this);
		this._context = undefined;
		super.hostDisconnected();
	}

	abstract translate(path: string): ReturnType<UmbValidationMessageTranslator['translate']>;
}
