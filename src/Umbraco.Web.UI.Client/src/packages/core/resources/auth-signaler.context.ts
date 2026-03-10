import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { Subject } from '@umbraco-cms/backoffice/external/rxjs';

export const UMB_AUTH_SIGNALER_CONTEXT = new UmbContextToken<UmbAuthSignalerContext>('UmbAuthSignalerContext');

/**
 * A lightweight context provided by the auth package and consumed by the resources package.
 * Acts as a bridge so that resource-layer concerns (e.g. HTTP interceptors) can react to
 * authentication state without taking a direct dependency on the auth package.
 */
export class UmbAuthSignalerContext extends UmbContextBase {
	#isAuthorized = new UmbBooleanState(false);
	readonly isAuthorized = this.#isAuthorized.asObservable();

	#timeoutRequest = new Subject<void>();
	/** Emits when an HTTP interceptor detects a 401 and needs the auth layer to show the login UI. */
	readonly timeoutRequest = this.#timeoutRequest.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_AUTH_SIGNALER_CONTEXT);
	}

	/** Called by the auth context to keep authorization state in sync. */
	setAuthorized(value: boolean) {
		this.#isAuthorized.setValue(value);
	}

	/** Called by HTTP interceptors when a 401 response is received. */
	requestTimeout() {
		this.#timeoutRequest.next();
	}
}
