import { UMB_FILTER_MANAGER_CONTEXT } from '../../filter-manager.context-token.js';
import { UMB_FILTER_VALUE_COORDINATOR_CONTEXT } from '../../value-resolver/filter-value-coordinator.context-token.js';
import type { UmbFilterActiveValueApi } from '../filter-active-value-api.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Base class for filter active value APIs. Registers the value with the filter value coordinator, exposes the
 * resolved value, and removes the value through the filter manager.
 *
 * Extend this class to create custom filter active value APIs.
 */
export abstract class UmbFilterActiveValueApiBase<TValue = unknown, TResolvedValue = unknown>
	extends UmbControllerBase
	implements UmbFilterActiveValueApi<TValue, TResolvedValue>
{
	#resolvedValue = new UmbObjectState<TResolvedValue | undefined>(undefined);
	readonly resolvedValue: Observable<TResolvedValue | undefined> = this.#resolvedValue.asObservable();

	#coordinator?: typeof UMB_FILTER_VALUE_COORDINATOR_CONTEXT.TYPE;
	#manager?: typeof UMB_FILTER_MANAGER_CONTEXT.TYPE;
	#connectPending = false;

	#alias?: string;
	get alias(): string | undefined {
		return this.#alias;
	}
	set alias(alias: string | undefined) {
		this.#alias = alias;
		this.#scheduleConnect();
	}

	unique?: string;

	#value?: TValue;
	get value(): TValue | undefined {
		return this.#value;
	}
	set value(value: TValue | undefined) {
		this.#value = value;
		this.#scheduleConnect();
	}

	constructor(host: UmbControllerHost) {
		super(host);
		this.consumeContext(UMB_FILTER_VALUE_COORDINATOR_CONTEXT, (coordinator) => {
			this.#coordinator = coordinator;
			this.#scheduleConnect();
		});
		this.consumeContext(UMB_FILTER_MANAGER_CONTEXT, (manager) => {
			this.#manager = manager;
		});
	}

	remove(): void {
		if (!this.#alias || this.unique === undefined) return;
		this.#manager?.clearActiveFilter(this.#alias, this.unique);
	}

	#scheduleConnect() {
		if (this.#connectPending) return;
		this.#connectPending = true;
		queueMicrotask(() => {
			this.#connectPending = false;
			this.#connect();
		});
	}

	#connect() {
		if (this.#alias === undefined) return;

		if (this.#coordinator) {
			this.#coordinator.register(this.#alias, this.#value);
			this.observe(
				this.#coordinator.observeResolvedValue(this.#alias, this.#value),
				(resolvedValue) => this.#resolvedValue.setValue(resolvedValue as TResolvedValue | undefined),
				'resolvedValue',
			);
		} else {
			this.#resolvedValue.setValue(this.#value as unknown as TResolvedValue);
		}
	}
}
