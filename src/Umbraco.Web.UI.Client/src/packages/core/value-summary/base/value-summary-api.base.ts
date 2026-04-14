import { UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT } from '../coordinator/value-summary-coordinator.context-token.js';
import type { UmbValueSummaryApi } from '../extensions/value-summary-api.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Abstract base class for value summary APIs.
 * Bridges the coordinator to the element.
 *
 * Receives `valueType` and `rawValue` via apiProps from the wrapper element.
 * Consumes the coordinator context to register for batch resolution and
 * exposes the result via the `value` observable.
 *
 * Extend this class to create custom value summary APIs.
 */
export abstract class UmbValueSummaryApiBase extends UmbControllerBase implements UmbValueSummaryApi {
	#value = new UmbObjectState<unknown>(undefined);
	readonly value: Observable<unknown> = this.#value.asObservable();

	#coordinator?: typeof UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT.TYPE;
	#valueType?: string;
	#rawValue?: unknown;
	#connectPending = false;

	constructor(host: UmbControllerHost) {
		super(host);
		this.consumeContext(UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT, (coordinator) => {
			this.#coordinator = coordinator;
			this.#scheduleConnect();
		});
	}

	set valueType(v: string | undefined) {
		this.#valueType = v;
		this.#scheduleConnect();
	}

	set rawValue(v: unknown) {
		this.#rawValue = v;
		this.#scheduleConnect();
	}

	/**
	 * Debounce via microtask so that when apiProps assigns valueType and value
	 * in sequence, we only call #connect() once with both values set.
	 */
	#scheduleConnect() {
		if (this.#connectPending) return;
		this.#connectPending = true;
		queueMicrotask(() => {
			this.#connectPending = false;
			this.#connect();
		});
	}

	#connect() {
		if (this.#valueType === undefined) return;

		if (this.#coordinator) {
			this.#coordinator.preRegister(this.#valueType, this.#rawValue);
			this.observe(
				this.#coordinator.observeResolvedValue(this.#valueType, this.#rawValue),
				(v) => this.#value.setValue(v),
				'value',
			);
		} else {
			// No coordinator — pass raw value through
			this.#value.setValue(this.#rawValue);
		}
	}
}
