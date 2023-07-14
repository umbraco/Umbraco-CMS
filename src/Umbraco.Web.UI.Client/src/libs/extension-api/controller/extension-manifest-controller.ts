import type { ManifestWithDynamicConditions } from '../types.js';
import { UmbBaseExtensionController } from './base-extension-controller.js';

export class UmbExtensionManifestController<
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions
> extends UmbBaseExtensionController<ManifestType> {
	protected async _conditionsAreGood() {
		return true;
	}

	protected async _conditionsAreBad() {
		// Destroy the element/class.
	}

	public equal(otherClass: UmbExtensionManifestController | undefined): boolean {
		return otherClass?.manifest === this.manifest;
	}
}
