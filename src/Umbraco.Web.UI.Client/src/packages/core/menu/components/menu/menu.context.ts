import type { ManifestMenu } from '../../menu.extension.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDeepState } from '@umbraco-cms/backoffice/observable-api';

export class UmbMenuContext {
	#manifest = new UmbDeepState<ManifestMenu | undefined>(undefined);
	public readonly manifest = this.#manifest.asObservable();
	public readonly alias = this.#manifest.asObservablePart((x) => x?.alias);

	public setManifest(manifest: ManifestMenu | undefined) {
		this.#manifest.setValue(manifest);
	}
}

export const UMB_MENU_CONTEXT = new UmbContextToken<UmbMenuContext>('UMB_MENU_CONTEXT');
