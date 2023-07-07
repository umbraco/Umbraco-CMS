import { UmbExtensionController } from './extension-controller.js';

export class UmbElementExtensionController extends UmbExtensionController {
	public component?: HTMLElement | null;

	/*
	constructor(host: UmbControllerHost, manifest: ManifestBase) {
		super(host, manifest);
	}
	*/

	protected _conditionsAreGood() {
		// Create the element/class.
	}

	protected _conditionsAreBad() {
		// Destroy the element/class.
	}
}
