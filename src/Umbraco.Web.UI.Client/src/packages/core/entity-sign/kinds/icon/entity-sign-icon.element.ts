import type { ManifestEntitySignIconKind } from './types.js';
import { css, customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbEntitySignElement } from '../../types.js';

@customElement('umb-entity-sign-icon')
export class UmbEntitySignIconElement extends UmbLitElement implements UmbEntitySignElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestEntitySignIconKind;

	override render() {
		return this.manifest
			? html`<umb-icon
					.name=${this.manifest.meta.iconName ?? 'icon-circle-dotted'}
					.color=${this.manifest.meta.iconColor}></umb-icon>`
			: nothing;
	}

	static override styles = [
		css`
			umb-icon {
				background: var(--sign-bundle-bg);
				border-radius: 50%;
			}
		`,
	];
}

export { UmbEntitySignIconElement as element };
