import { UmbViewController } from './view.controller.js';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';

export class UmbViewContext extends UmbViewController {
	constructor(host: UmbClassInterface, viewAlias: string | null) {
		super(host, viewAlias);
		this.provideAt(host);
	}
}
