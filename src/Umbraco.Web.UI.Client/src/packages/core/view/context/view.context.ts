import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import { UmbViewController } from './view.controller.js';

export class UmbViewContext extends UmbViewController {
	constructor(host: UmbClassInterface, viewAlias: string | null) {
		super(host, viewAlias);
		this.provideAt(host);
	}
}
