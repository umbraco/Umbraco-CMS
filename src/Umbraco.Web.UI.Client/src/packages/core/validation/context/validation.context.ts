import { UmbValidationController } from '../controllers/validation.controller.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
/**
 * Validation Context is the core of Validation.
 * It hosts Validators that has to validate for the context to be valid.
 * It can also be used as a Validator as part of a parent Validation Context.
 */
export class UmbValidationContext extends UmbValidationController {
	constructor(host: UmbControllerHost) {
		// This is overridden to avoid setting a controllerAlias, this might make sense, but currently i want to leave it out. [NL]
		super(host);
		this.provideContext(UMB_VALIDATION_CONTEXT, this);
	}

	/**
	 * Provides the validation context to the current host, if not already provided to a different host.
	 * @deprecated No need to provide, this happens automatically. (Do notice this was necessary in 14.3.-rc, but removed in 14.3 release)
	 * @returns instance {UmbValidationController} - Returns it self.
	 */
	provide(): UmbValidationController {
		return this;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	override provideAt(controllerHost: UmbClassInterface): void {
		throw new Error(
			'UmbValidationContext cannot be used to provide at a different host. Use the UmbValidationController instead.',
		);
	}
}
