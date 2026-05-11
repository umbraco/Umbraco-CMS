import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Use UmbChangeEvent instead, this will be removed as of v.18.0.0
 */
export class UmbPropertyValueChangeEvent extends UmbChangeEvent {
	constructor() {
		super();
		new UmbDeprecation({
			removeInVersion: '18.0.0',
			deprecated: 'UmbPropertyValueChangeEvent',
			solution: 'Use UmbChangeEvent instead',
		}).warn();
	}
}
