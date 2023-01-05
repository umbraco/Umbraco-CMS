import { UmbResourceController } from './resource.controller';

export function tryExecute<T>(promise: Promise<T>) {
	return UmbResourceController.tryExecute<T>(promise);
}
