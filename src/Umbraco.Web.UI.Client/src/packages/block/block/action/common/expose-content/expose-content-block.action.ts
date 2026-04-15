import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import type { UmbBlockActionArgs } from '../../types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbExposeContentBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	#context?: typeof UMB_BLOCK_ENTRY_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbBlockActionArgs<MetaBlockActionDefaultKind>) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.#context = context;
		});
	}

	override async execute() {
		this.#context?.expose();
	}
}

export { UmbExposeContentBlockAction as api };
