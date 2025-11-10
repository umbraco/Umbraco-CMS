import type { UmbPropertyValuePresentationDisplayOption } from '../../core/property-value-presentation/property-value-presentation.extension.js';
import { property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbPropertyValuePresentationBaseElement<TValue = string> extends UmbLitElement {
	@property()
	alias: string = '';

	@property()
	display?: UmbPropertyValuePresentationDisplayOption;

	@property({ type: Object })
	value?: TValue;
}
