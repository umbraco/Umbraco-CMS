import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { Subject } from '@umbraco-cms/backoffice/external/rxjs';

export const UMB_AUTH_SIGNALER_CONTEXT = new UmbContextToken<UmbAuthSignalerContext>('UmbAuthSignalerContext');

/**
 * A lightweight bridge context owned by {@link UmbApiInterceptorController} (resources package)
 * and consumed by the auth package. Allows resource-layer concerns (e.g. HTTP interceptors) to
 * signal authentication state without creating a circular dependency on the auth package.
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
