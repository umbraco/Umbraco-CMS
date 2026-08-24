import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import type { UmbBlockActionArgs } from '../../types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';
import { UmbDataPathGeneratorForBlockElementData } from '../../../validation/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';

/**
 * Block action that navigates to the block's content editor workspace.
 * Exposes the workspace edit path via `getHref()` and the content validation data path via `getValidationDataPath()`.
 */
export class UmbEditContentBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	#context?: typeof UMB_BLOCK_ENTRY_CONTEXT.TYPE;
	#contextReady: Promise<void>;
	#resolveContext!: () => void;

	constructor(host: UmbControllerHost, args: UmbBlockActionArgs<MetaBlockActionDefaultKind>) {
		super(host, args);

		this.#contextReady = new Promise<void>((resolve) => {
			this.#resolveContext = resolve;
		});

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.#context = context;
			this.#resolveContext();
		});
	}

	async getHrefObservable(): Promise<Observable<string | undefined> | undefined> {
		await this.#contextReady;
		return this.#context?.workspaceEditContentPath;
	}

	async getValidationDataPathObservable(): Promise<Observable<string | undefined> | undefined> {
		await this.#contextReady;
		if (!this.#context) return undefined;
		return mergeObservables([this.#context.contentKey], ([contentKey]) => {
			if (!contentKey) return undefined;
			return UmbDataPathGeneratorForBlockElementData('contentData', { key: contentKey });
		});
	}
}

export { UmbEditContentBlockAction as api };
