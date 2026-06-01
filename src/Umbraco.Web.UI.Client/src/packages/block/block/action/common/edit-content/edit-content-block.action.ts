import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import type { UmbBlockActionArgs } from '../../types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UmbDataPathBlockElementDataQuery } from '../../../validation/data-path-element-data-query.function.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Block action that navigates to the block's content editor workspace.
 * Exposes the workspace edit path via `getHref()` / `hrefObservable` and the content validation
 * data path via `getValidationDataPath()` / `validationDataPathObservable`.
 * The observable variants update reactively (e.g. after disconnect from Element Library),
 * whereas the promise variants resolve once for consumers that call them imperatively.
 */
export class UmbEditContentBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	#context?: typeof UMB_BLOCK_ENTRY_CONTEXT.TYPE;
	#contextReady: Promise<void>;
	#resolveContext!: () => void;

	readonly #href = new UmbStringState(undefined);
	readonly hrefObservable = this.#href.asObservable();

	readonly #validationDataPath = new UmbStringState(undefined);
	readonly validationDataPathObservable = this.#validationDataPath.asObservable();

	constructor(host: UmbControllerHost, args: UmbBlockActionArgs<MetaBlockActionDefaultKind>) {
		super(host, args);

		this.#contextReady = new Promise<void>((resolve) => {
			this.#resolveContext = resolve;
		});

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.#context = context;
			if (!context) return;
			this.#resolveContext();

			this.observe(
				context.workspaceEditContentPath,
				(path) => this.#href.setValue(path || undefined),
				'observeHref',
			);

			this.observe(
				context.contentKey,
				(contentKey) => {
					this.#validationDataPath.setValue(
						contentKey
							? `$.contentData[${UmbDataPathBlockElementDataQuery({ key: contentKey })}]`
							: undefined,
					);
				},
				'observeValidationDataPath',
			);
		});
	}

	override async getHref() {
		await this.#contextReady;
		const path = await this.observe(this.#context?.workspaceEditContentPath)?.asPromise();
		return path || undefined;
	}

	override async getValidationDataPath() {
		await this.#contextReady;
		const contentKey = await this.observe(this.#context?.contentKey)?.asPromise();
		if (!contentKey) return undefined;
		return `$.contentData[${UmbDataPathBlockElementDataQuery({ key: contentKey })}]`;
	}
}

export { UmbEditContentBlockAction as api };
