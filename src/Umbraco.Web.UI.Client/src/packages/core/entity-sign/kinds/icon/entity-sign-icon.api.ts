import type { UmbEntitySignApi, UmbEntitySignApiArgs } from '../../extensions/entity-sign-api.interface.js';
import type { MetaEntitySignIconKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbIconEntitySignApi implements UmbEntitySignApi {
	#label: string;

	constructor(host: UmbControllerHost, args: UmbEntitySignApiArgs<MetaEntitySignIconKind>) {
		this.#label = args.meta.label;
	}

	getLabel(): string {
		return this.#label;
	}

	destroy(): void {}
}

export { UmbIconEntitySignApi as api };
