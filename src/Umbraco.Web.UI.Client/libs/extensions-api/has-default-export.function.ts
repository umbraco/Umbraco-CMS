import type { HTMLElementConstructor } from '../models';

export function hasDefaultExport(object: unknown): object is { default: HTMLElementConstructor } {
	return typeof object === 'object' && object !== null && 'default' in object;
}
