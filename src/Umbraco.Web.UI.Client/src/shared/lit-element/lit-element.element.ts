import { LitElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

export class UmbLitElement extends UmbElementMixin(LitElement) {
	// Make `dir` and `lang` reactive properties so they react to language changes:
	@property() dir = '';
	@property() lang = '';
}
