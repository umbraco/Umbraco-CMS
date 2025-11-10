import type { UmbPropertyValuePresentationDisplayOption } from '../../core/property-value-presentation/property-value-presentation.extension.js';
import { property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbPropertyValuePresentationBaseElement extends UmbLitElement {
	@property()
	alias: string = '';

	@property()
	display?: UmbPropertyValuePresentationDisplayOption;

	@property()
	value: any;
}
