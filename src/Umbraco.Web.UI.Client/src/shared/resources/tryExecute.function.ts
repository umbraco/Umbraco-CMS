import { UmbResourceController } from './resource.controller.js';

export function tryExecute<T>(promise: Promise<T>) {
	return UmbResourceController.tryExecute<T>(promise);
}
