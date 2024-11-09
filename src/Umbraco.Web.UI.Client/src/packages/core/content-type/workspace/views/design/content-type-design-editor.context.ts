import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export const UMB_CONTENT_TYPE_DESIGN_EDITOR_CONTEXT = new UmbContextToken<
	UmbContentTypeDesignEditorContext,
	UmbContentTypeDesignEditorContext
>('UmbContentTypeDesignEditorContext');

export class UmbContentTypeDesignEditorContext extends UmbContextBase<UmbContentTypeDesignEditorContext> {
	#isSorting = new UmbBooleanState(false);
	readonly isSorting = this.#isSorting.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_CONTENT_TYPE_DESIGN_EDITOR_CONTEXT);
	}

	getIsSorting() {
		return this.#isSorting.getValue();
	}

	setIsSorting(isSorting: boolean) {
		this.#isSorting.setValue(isSorting);
	}

	public override destroy(): void {
		this.#isSorting.destroy();
		super.destroy();
	}
}

export { UmbContentTypeDesignEditorContext as api };
