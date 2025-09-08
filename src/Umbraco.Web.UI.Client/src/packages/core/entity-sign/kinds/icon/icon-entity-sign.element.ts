import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestEntitySignIconKind } from '../../types';

@customElement('umb-entity-sign-icon')
export class UmbEntitySignIconElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestEntitySignIconKind;

	override render() {
		return this.manifest
			? html`<umb-icon
					name=${this.manifest.meta.iconName ?? 'icon-circle-dotted'}
					title="TODO: ... get it from the API"></umb-icon>`
			: nothing;
	}
}

export { UmbEntitySignIconElement as element };
