import type { ManifestWithDynamicConditions } from '../types.js';
import { UmbExtensionController } from './extension-controller.js';

export class UmbManifestExtensionController<
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions
> extends UmbExtensionController<ManifestType> {
	protected async _conditionsAreGood() {
		return true;
	}

	protected async _conditionsAreBad() {
		// Destroy the element/class.
	}

	public equal(otherClass: UmbManifestExtensionController | undefined): boolean {
		return otherClass?.manifest === this.manifest;
	}
}
