import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUfmRenderContext extends UmbContextBase {
	#value = new UmbObjectState<unknown>(undefined);
	readonly value = this.#value.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_UFM_RENDER_CONTEXT);
	}

	getValue() {
		return this.#value.getValue();
	}

	setValue(value: unknown | undefined) {
		this.#value.setValue(value);
	}
}

export default UmbUfmRenderContext;

export const UMB_UFM_RENDER_CONTEXT = new UmbContextToken<UmbUfmRenderContext>('UmbUfmRenderContext');
