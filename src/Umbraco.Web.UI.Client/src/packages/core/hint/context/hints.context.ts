import type { UmbHint, UmbIncomingHintBase } from '../types.js';
import { UMB_HINT_CONTEXT } from './hint.context-token.js';
import { UmbHintController } from './hints.controller.js';
import type { UmbPartialSome } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbHintContext<
	HintType extends UmbHint = UmbHint,
	IncomingHintType extends UmbIncomingHintBase = UmbPartialSome<HintType, 'unique' | 'weight' | 'path'>,
> extends UmbHintController<HintType, IncomingHintType> {
	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(UMB_HINT_CONTEXT, this as unknown as UmbHintContext);
	}
}
