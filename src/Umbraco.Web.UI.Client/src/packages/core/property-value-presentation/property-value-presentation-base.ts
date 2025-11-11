import { property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbPropertyValuePresentationBaseElement<TValue = string> extends UmbLitElement {
	@property({ type: Object })
	value?: TValue;
}
