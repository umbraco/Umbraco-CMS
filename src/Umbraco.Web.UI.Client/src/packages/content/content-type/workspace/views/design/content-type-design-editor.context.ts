import { UMB_CONTENT_TYPE_DESIGN_EDITOR_CONTEXT } from './content-type-design-editor.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbContentTypeDesignEditorContext extends UmbContextBase {
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
